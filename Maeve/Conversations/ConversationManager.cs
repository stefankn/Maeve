using Maeve.Database;
using Maeve.ModelContextProtocol;
using Maeve.ModelProviders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using ILogger = Maeve.Logging.ILogger;

namespace Maeve.Conversations;

public class ConversationManager(
    IChatClient chatClient,
    IDbContextFactory<DataContext> dbContextFactory,
    ILogger logger,
    IMcpConfigurator mcpConfigurator,
    IModelProvider modelProvider
    ): IConversationManager {
    
    // - Private Properties
    
    private readonly Dictionary<string, IConversationContext> _conversationContexts = new();
    
    
    // - Properties

    public Conversation[] Conversations {
        get {
            using var dataContext = dbContextFactory.CreateDbContext();
            return dataContext.Conversations.Where(c => !ActiveConversations.Select(ac => ac.Id).Contains(c.Id)).ToArray();
        }
    }
    public IConversationContext[] ActiveConversations => _conversationContexts.Values.ToArray();
    public IConversationContext? FocusedConversation { get; private set; }
    public event EventHandler<IConversationContext?>? OnConversationFocus;


    // - Functions

    public IConversationContext StartConversation(string conversationId) {
        if (_conversationContexts.TryGetValue(conversationId, out var conversation)) {
            FocusedConversation = conversation;
            OnConversationFocus?.Invoke(this, conversation);
            return conversation;
        }

        var conversationContext = new ConversationContext(conversationId, chatClient, dbContextFactory, logger, mcpConfigurator, modelProvider);
        _conversationContexts[conversationId] = conversationContext;

        FocusedConversation = conversationContext;
        OnConversationFocus?.Invoke(this, conversationContext);

        return conversationContext;
    }

    public void LeaveConversation(IConversationContext conversationContext) {
        if (FocusedConversation != conversationContext) return;
        
        FocusedConversation = null;
        OnConversationFocus?.Invoke(this, null);
    }

    public async Task Delete(IConversationContext conversationContext) {
        if (conversationContext.Id == null) return;
        
        _conversationContexts.Remove(conversationContext.Id);
        
        await using var dataContext = await dbContextFactory.CreateDbContextAsync();
        var conversationToDelete = await dataContext.Conversations.FindAsync(conversationContext.Id);
        if (conversationToDelete != null) {
            dataContext.Conversations.Remove(conversationToDelete);
            await dataContext.SaveChangesAsync();
        }
    }
}