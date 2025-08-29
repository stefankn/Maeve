using Maeve.Database;
using Maeve.Logging;
using Maeve.ModelContextProtocol;
using Maeve.ModelProviders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using ILogger = Maeve.Logging.ILogger;
using Message = Maeve.Database.Message;
using Tool = Maeve.Database.Tool;
using ChatRole = Microsoft.Extensions.AI.ChatRole;

namespace Maeve.Conversations;

public sealed class ConversationContext: IConversationContext {
    
    // - Private Properties

    private readonly IDbContextFactory<DataContext> _dbContextFactory;
    private readonly ILogger _logger;
    private readonly IMcpConfigurator _mcpConfigurator;
    private readonly IModelProvider _modelProvider;
    private bool _isThinking;

    private readonly IChatClient _chatClient;
    private readonly List<Message> _messages = [];
    private readonly List<Tool> _usedTools = [];
    
    
    // - Properties
    
    public event EventHandler<string?>? OnThoughts;
    public event EventHandler<Tool?>? OnToolInvocation;
    public event EventHandler<Message>? OnNewMessage;
    public event EventHandler<string?>? OnResponse;

    public string Id { get; }
    public string Title { get; }
    public IModelProvider ModelProvider => _modelProvider;
    public bool IsResponding { get; private set; }
    public Message[] Messages => _messages.Where(m => m.Role != Role.System).ToArray();
    public string? Thoughts { get; private set; }
    public string? Response { get; private set; }
    public Tool[] UsedTools => _usedTools.ToArray();
    
    
    // - Construction

    public ConversationContext(
        string conversationId,
        IChatClientFactory chatClientFactory,
        IDbContextFactory<DataContext> dbContextFactory,
        ILogger logger,
        IMcpConfigurator mcpConfigurator,
        IModelProviderFactory modelProviderFactory
        ) {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
        _mcpConfigurator = mcpConfigurator;
        
        var dataContext = dbContextFactory.CreateDbContext();
        var conversation = dataContext.Conversations
            .Include(c => c.Messages)
            .FirstOrDefault(c => c.Id == conversationId);
        if (conversation == null) throw new Exception("Conversation not found");

        _modelProvider = modelProviderFactory.CreateModelProvider(conversation.Provider);
        _chatClient = chatClientFactory.CreateChatClient(conversation.Provider);
        
        Id = conversationId;
        Title = conversation.Title;
        
        // Construct history
        _messages.AddRange(conversation.Messages.OrderBy(m => m.CreatedAt));
        
        var lastMessage = _messages.LastOrDefault();
        if (lastMessage != null && lastMessage.Role == Role.User) {
            IsResponding = true;
            _ = Task.Run(async () => await PerformSend());
        }
    }
    
    
    // - Functions

    public async Task SendMessage(string query, Document? document = null) {
        if (query.Trim() == "") return;
        
        var messagesToStore = new List<Message>();

        if (document != null) {
            var systemMessage = new Message {
                Content = $"Use the filename {document.Filename} when using a tool to query a document.",
                CreatedAt = DateTime.Now,
                Role = Role.System
            };
            _messages.Add(systemMessage);
            messagesToStore.Add(systemMessage);
            
            _logger.Information($"Adding system message: {systemMessage.Content}", LogCategory.Llm, true);
        }
        
        var message = new Message {
            Content = query,
            CreatedAt = DateTime.Now,
            Role = Role.User
        };
        
        _messages.Add(message);
        messagesToStore.Add(message);

        await using var dataContext = await _dbContextFactory.CreateDbContextAsync();
        var conversation = await dataContext.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == Id);
        if (conversation == null) return;

        try {
            messagesToStore.ForEach(m => conversation.Messages.Add(m));
            conversation.UpdatedAt = DateTime.Now;
            await dataContext.SaveChangesAsync();
            
            OnNewMessage?.Invoke(this, message);
            IsResponding = true;
            
            await PerformSend();
        } catch (Exception e) {
            _logger.Error("Failed to send message", LogCategory.Llm, consoleLog: true);
            _logger.Error(e.ToString(), LogCategory.Llm);
            throw;
        }
    }
    
    
    // - Private Functions
    
    private async Task PerformSend() {
        var messages = _messages.Select(m => new ChatMessage(new ChatRole(m.Role.Key()), m.Content));
        var tools = _mcpConfigurator.AvailableTools.ToList();

        var options = new ChatOptions {
            Tools = [..tools],
            Instructions = "You can perform multiple tool calls in a single response.",
            ToolMode = ChatToolMode.Auto,
            AllowMultipleToolCalls = true,
            ModelId = _modelProvider.DefaultModelId,
            MaxOutputTokens = _modelProvider.MaxOutputTokens
        };


        try {
            await foreach (var update in _chatClient.GetStreamingResponseAsync(messages, options)) {
                HandleUpdate(update);
            }
        } catch (Exception e) {
            _logger.Error($"Streaming response failure, {e}", LogCategory.Llm, consoleLog: true);
            _logger.Error($"Streaming response failure, {e}", LogCategory.Llm);
            return;
        }

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
                _logger.Error("Failed to save response", LogCategory.Llm, consoleLog: true);
                _logger.Error($"Error saving response, {e}", LogCategory.Llm);
            }
        }

        OnResponse?.Invoke(this, Response);
    }
    
    private void HandleUpdate(ChatResponseUpdate update) {
        foreach (var content in update.Contents) {
            switch (content) {
                case TextReasoningContent reasoningContent:
                    if (Thoughts == null) {
                        Thoughts = reasoningContent.Text;
                    } else {
                        Thoughts += reasoningContent.Text;
                    }
                        
                    OnThoughts?.Invoke(this, Thoughts);
                    break;
                case TextContent textContent:
                    // In case the model doesn't use reasoning functionality but the reasoning is
                    // done in the general response
                    switch (textContent.Text) {
                        case "<think>":
                            _isThinking = true;
                            continue;
                        case "</think>":
                            _isThinking = false;
                            continue;
                    }

                    if (_isThinking) {
                        if (Thoughts == null) {
                            Thoughts = textContent.Text;
                        } else {
                            Thoughts += textContent.Text;
                        }
                        
                        OnThoughts?.Invoke(this, Thoughts);
                    } else {
                        if (Response == null) {
                            if (textContent.Text.Trim() == "") {
                                continue;
                            }
                            Response = textContent.Text;
                        } else {
                            Response += textContent.Text;
                        }
                        
                        OnResponse?.Invoke(this, Response);
                    }
                    
                    break;
                case FunctionCallContent functionCall:
                    var arguments = functionCall.Arguments?.Select(a => new Tool.Argument {
                        Key = a.Key, Value = a.Value?.ToString()
                    }).ToList();

                    var tool = new Tool {
                        Function = functionCall.Name,
                        Arguments = arguments ?? []
                    };
                    _logger.Information($"Tool call - {tool.Description}", LogCategory.Tools, consoleLog: true);
                    _usedTools.Add(tool);
                    
                    OnToolInvocation?.Invoke(this, tool);
                    break;
            }
        }
    }
}