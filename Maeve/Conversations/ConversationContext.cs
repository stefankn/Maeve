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

public sealed class ConversationContext: IConversationContext {
    
    // - Private Properties

    private readonly IDbContextFactory<DataContext> _dbContextFactory;
    private readonly ILogger _logger;
    private readonly IWebHostEnvironment _environment;

    private readonly Chat _chat;
    private readonly List<Message> _messages = [];
    private readonly List<Tool> _usedTools = [];
    
    
    // - Properties
    
    public event EventHandler<string?>? OnThoughts;
    public event EventHandler<Tool?>? OnToolInvocation;
    public event EventHandler<Message>? OnNewMessage;
    public event EventHandler<string?>? OnResponse;

    public string Id { get; }
    public string Title { get; }
    public bool IsResponding { get; private set; }
    public Message[] Messages => _messages.ToArray();
    public string? Thoughts { get; private set; }
    public string? Response { get; private set; }
    public Tool[] UsedTools => _usedTools.ToArray();
    
    
    // - Construction

    public ConversationContext(
        string conversationId,
        IOllamaApiClient ollamaApiClient,
        IDbContextFactory<DataContext> dbContextFactory,
        ILogger logger,
        IWebHostEnvironment environment
        ) {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
        _environment = environment;
        
        var dataContext = dbContextFactory.CreateDbContext();
        var conversation = dataContext.Conversations
            .Include(c => c.Messages)
            .FirstOrDefault(c => c.Id == conversationId);
        if (conversation == null) throw new Exception("Conversation not found");
        
        Id = conversationId;
        Title = conversation.Title;

        _chat = new Chat(ollamaApiClient) { Think = true };
        _chat.OnThink += OnThink;
        _chat.OnToolCall += OnToolCall;
        _chat.OnToolResult += OnToolResult;
        
        // Construct history
        _messages.AddRange(conversation.Messages.OrderBy(m => m.CreatedAt));
        _chat.Messages.AddRange(_messages.Select(m => new OllamaMessage(new ChatRole(m.Role.Key()), m.Content)));
        
        var lastMessage = _messages.LastOrDefault();
        if (lastMessage is { Role: Role.User }) {
            IsResponding = true;
            _ = Task.Run(async () => await PerformSend(lastMessage));
        }
    }
    
    
    // - Functions

    public async Task SendMessage(string query) {
        if (query.Trim() == "") return;
        
        var message = new Message {
            Content = query,
            CreatedAt = DateTime.Now,
            Role = Role.User
        };
        
        _messages.Add(message);
        OnNewMessage?.Invoke(this, message);
        IsResponding = true;
        
        await using var dataContext = await _dbContextFactory.CreateDbContextAsync();
        var conversation = await dataContext.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == Id);
        if (conversation == null) return;

        try {
            conversation.Messages.Add(message);
            conversation.UpdatedAt = DateTime.Now;
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
        
        if (Response != null) {
            try {
                await using var dataContext = await _dbContextFactory.CreateDbContextAsync();
                var conversation = await dataContext.Conversations
                    .Include(c => c.Messages)
                    .FirstOrDefaultAsync(c => c.Id == Id);
                if (conversation == null) return;

                var assistantMessage = new Message {
                    Role = Role.Assistant,
                    Content = Response,
                    Thoughts = Thoughts,
                    CreatedAt = DateTime.Now
                };
                assistantMessage.Tools.AddRange(_usedTools);
                _messages.Add(assistantMessage);
                IsResponding = false;
                
                Response = null;
                Thoughts = null;
                _usedTools.Clear();
                
                OnNewMessage?.Invoke(this, assistantMessage);
            
                conversation.Messages.Add(assistantMessage);
                conversation.UpdatedAt = DateTime.Now;
                await dataContext.SaveChangesAsync();
            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }
        
        OnResponse?.Invoke(this, Response);
    }
    
    private async Task AwaitTokens(IAsyncEnumerable<string> response) {
        await foreach (var token in response) {
            if (token.Trim() == "") continue;
            
            if (Response == null) {
                Response = token;
            } else {
                Response += token;
            }

            if (Response.Trim() != "") {
                OnToolInvocation?.Invoke(this, null);
                OnThoughts?.Invoke(this, null);
            }
 
            OnResponse?.Invoke(this, Response);
        }
    }
    
    private void OnThink(object? sender, string e) {
        if (Thoughts == null) {
            Thoughts = e;
        } else {
            Thoughts += e;
        }

        OnThoughts?.Invoke(this, Thoughts);
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
        OnToolInvocation?.Invoke(this, tool);
    }
    
    private void OnToolResult(object? sender, ToolResult e) {
        
    }
}