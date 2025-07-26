using System.Text.Json;
using Maeve.Database;
using Maeve.Logging;
using Microsoft.EntityFrameworkCore;
using OllamaSharp;
using OllamaSharp.ModelContextProtocol;
using OllamaSharp.ModelContextProtocol.Server;
using OllamaSharp.Models.Chat;
using OllamaSharp.Tools;
using ILogger = Maeve.Logging.ILogger;
using Message = Maeve.Database.Message;
using OllamaMessage = OllamaSharp.Models.Chat.Message;
using Tool = Maeve.Database.Tool;

namespace Maeve.Conversations;

public class ConversationContext: IConversationContext {
    
    // - Private Properties

    private readonly IOllamaApiClient _ollamaApiClient;
    private readonly ILogger _logger;
    private readonly IDbContextFactory<DataContext> _dbContextFactory;
    private readonly IWebHostEnvironment _environment;

    private readonly Chat _chat;
    private readonly List<Message> _messages = [];
    private string? _thoughts;
    private string? _response;
    private readonly List<Tool> _usedTools = [];
    
    
    // - Properties

    public event EventHandler<string?>? Thoughts;
    public event EventHandler<Tool?>? ToolInvoked;
    public event EventHandler<Message>? NewMessage;
    public event EventHandler<string?>? Response;

    public Message[] Messages => _messages.ToArray();
    
    
    // - Construction

    public ConversationContext(
        IOllamaApiClient ollamaApiClient,
        IDbContextFactory<DataContext> dbContextFactory,
        ILogger logger,
        IWebHostEnvironment environment
        ) {
        
        _ollamaApiClient = ollamaApiClient;
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _environment = environment;
        
        _chat = new Chat(ollamaApiClient) { Think = true };
        _chat.OnThink += OnThink;
        _chat.OnToolCall += OnToolCall;
        _chat.OnToolResult += OnToolResult;
    }
    
    
    // - Functions

    public async Task StartConversation() {
        await using var dataContext = await _dbContextFactory.CreateDbContextAsync();

        // Fetch history
        _messages.AddRange(dataContext.Messages.AsNoTracking().OrderBy(m => m.CreatedAt));
        _chat.Messages.AddRange(_messages.Select(m => new OllamaMessage(new ChatRole(m.Role.Key()), m.Content)));
        
        try {
            var version = await _ollamaApiClient.GetVersionAsync();
            _logger.Information($"Ollama version: {version}", LogCategory.Llm, consoleLog: true);
        } catch (Exception e) {
            _logger.Error("Failed to connect to Ollama", LogCategory.Llm, consoleLog: true);
            _logger.Error(e.ToString(), LogCategory.Llm);
        }
    }

    public async Task SendMessage(string query) {
        if (query.Trim() == "") return;
        
        var message = new Message {
            Content = query,
            CreatedAt = DateTime.Now,
            Role = Role.User
        };
        
        _messages.Add(message);
        NewMessage?.Invoke(this, message);

        try {
            await using var dataContext = await _dbContextFactory.CreateDbContextAsync();
            dataContext.Messages.Add(message);
            await dataContext.SaveChangesAsync();
            await PerformSend(message);
        } catch (Exception e) {
            _logger.Error("Failed to send message", LogCategory.Llm, consoleLog: true);
            _logger.Error(e.ToString(), LogCategory.Llm);
            throw;
        }
    }
    
    
    // - Private Functions

    private async Task PerformSend(Message message) {
        var tools = await GetTools();
        var response = _chat.SendAsync(message.Content, tools);
        await AwaitTokens(response);
        
        if (_response != null) {
            var assistantMessage = new Message {
                Role = Role.Assistant,
                Content = _response,
                Thoughts = _thoughts,
                Tools = _usedTools,
                CreatedAt = DateTime.Now
            };
            _messages.Add(assistantMessage);
            NewMessage?.Invoke(this, assistantMessage);
            
            await using var dataContext = await _dbContextFactory.CreateDbContextAsync();
            dataContext.Messages.Add(assistantMessage);
            await dataContext.SaveChangesAsync();
        }
        
        _response = null;
        _thoughts = null;
        _usedTools.Clear();
        
        Response?.Invoke(this, _response);
    }
    
    private async Task AwaitTokens(IAsyncEnumerable<string> response) {
        await foreach (var token in response) {
            if (_response == null) {
                _response = token;
            } else {
                _response += token;
            }

            if (_response.Trim() != "") {
                ToolInvoked?.Invoke(this, null);
                Thoughts?.Invoke(this, null);
            }
 
            Response?.Invoke(this, _response);
        }
    }
    
    private void OnThink(object? sender, string e) {
        if (_thoughts == null) {
            _thoughts = e;
        } else {
            _thoughts += e;
        }

        Thoughts?.Invoke(this, _thoughts);
    }
    
    private async Task<McpClientTool[]> GetTools() {
        var config = Path.Combine(_environment.ContentRootPath, "mcp_server_config.json");
        return await Tools.GetFromMcpServers(config);
    }
    
    private void OnToolCall(object? toolCall, OllamaMessage.ToolCall call) {
        if (call.Function?.Name == null) return;

        var arguments = call.Function.Arguments?
            .Select(arg => new Tool.Argument { Key = arg.Key, Value = arg.Value?.ToString() })
            .ToList();
        
        var tool = new Tool {
            Function = call.Function.Name,
            Arguments = arguments ?? []
        };
        _logger.Information($"Tool call - {tool.Description}", LogCategory.Tools, consoleLog: true);
        
        _usedTools.Add(tool);
        ToolInvoked?.Invoke(this, tool);
    }
    
    private void OnToolResult(object? sender, ToolResult e) {
        
    }
}