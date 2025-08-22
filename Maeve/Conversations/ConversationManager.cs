using Maeve.Database;
using Maeve.ModelContextProtocol;
using Maeve.ModelProviders;
using Microsoft.EntityFrameworkCore;
using ILogger = Maeve.Logging.ILogger;

namespace Maeve.Conversations;

public class ConversationManager(
    IChatClientFactory chatClientFactory,
    IDbContextFactory<DataContext> dbContextFactory,
    ILogger logger,
    IMcpConfigurator mcpConfigurator,
    IModelProviderFactory modelProviderFactory
    ): IConversationManager {
    
    // - Private Properties
    
    private readonly Dictionary<string, IConversationContext> _conversationContexts = new();
    
    
    // - Properties

    public Conversation[] Conversations {
        get {
            using var dataContext = dbContextFactory.CreateDbContext();
            return dataContext.Conversations.OrderByDescending(c => c.UpdatedAt).ToArray();
        }
    }
    public IConversationContext? FocusedConversation { get; private set; }
    
    public event EventHandler<IConversationContext?>? OnConversationFocus;
    public event EventHandler? OnConversationUpdate;


    // - Functions

    public IConversationContext StartConversation(string conversationId) {
        if (_conversationContexts.TryGetValue(conversationId, out var conversation)) {
            FocusedConversation = conversation;
            OnConversationFocus?.Invoke(this, conversation);
            return conversation;
        }
        
        var conversationContext = new ConversationContext(conversationId, chatClientFactory, dbContextFactory, logger, mcpConfigurator, modelProviderFactory);
        conversationContext.OnNewMessage += OnNewConversationMessage;
        _conversationContexts[conversationId] = conversationContext;

        FocusedConversation = conversationContext;
        OnConversationFocus?.Invoke(this, conversationContext);

        return conversationContext;
    }

    private void OnNewConversationMessage(object? sender, Message e) {
        OnConversationUpdate?.Invoke(this, EventArgs.Empty);
    }

    public void LeaveConversation(IConversationContext conversationContext) {
        if (FocusedConversation != conversationContext) return;
        
        FocusedConversation = null;
        OnConversationFocus?.Invoke(this, null);
    }

    public async Task Delete(IConversationContext conversationContext) {
        if (conversationContext.Id == null) return;
        
        conversationContext.OnNewMessage -= OnNewConversationMessage;
        _conversationContexts.Remove(conversationContext.Id);
        
        await using var dataContext = await dbContextFactory.CreateDbContextAsync();
        var conversationToDelete = await dataContext.Conversations.FindAsync(conversationContext.Id);
        if (conversationToDelete != null) {
            dataContext.Conversations.Remove(conversationToDelete);
            await dataContext.SaveChangesAsync();
        }
    }
}