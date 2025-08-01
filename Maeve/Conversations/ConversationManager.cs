using Maeve.Database;
using Microsoft.EntityFrameworkCore;
using OllamaSharp;
using ILogger = Maeve.Logging.ILogger;

namespace Maeve.Conversations;

public class ConversationManager(
    IOllamaApiClient ollamaApiClient,
    IDbContextFactory<DataContext> dbContextFactory,
    ILogger logger,
    IWebHostEnvironment environment
    ): IConversationManager {
    
    // - Private Properties
    
    private readonly Dictionary<string, IConversationContext> _conversationContexts = new();
    
    
    // - Properties

    public Conversation[] Conversations {
        get {
            using var dataContext = dbContextFactory.CreateDbContext();
            return dataContext.Conversations.ToArray();
        }
    }
    public IConversationContext[] ActiveConversations => _conversationContexts.Values.ToArray();
    public IConversationContext? FocusedConversation { get; private set; }

    
    // - Functions

    public IConversationContext StartConversation(string conversationId) {
        if (_conversationContexts.TryGetValue(conversationId, out var conversation)) {
            FocusedConversation = conversation;
            return conversation;
        }

        var conversationContext = new ConversationContext(conversationId, ollamaApiClient, dbContextFactory, logger, environment);
        _conversationContexts[conversationId] = conversationContext;

        FocusedConversation = conversationContext;

        return conversationContext;
    }

    public async Task Delete(IConversationContext conversationContext) {
        _conversationContexts.Remove(conversationContext.Id);
        
        await using var dataContext = await dbContextFactory.CreateDbContextAsync();
        var conversationToDelete = await dataContext.Conversations.FindAsync(conversationContext.Id);
        if (conversationToDelete != null) {
            dataContext.Conversations.Remove(conversationToDelete);
            await dataContext.SaveChangesAsync();
        }
    }
}