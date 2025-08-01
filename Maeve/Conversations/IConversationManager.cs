using Maeve.Database;

namespace Maeve.Conversations;

public interface IConversationManager {
    
    // - Properties
    
    public Conversation[] Conversations { get; }
    public IConversationContext[] ActiveConversations { get; }
    public IConversationContext? FocusedConversation { get; }
    
    
    // - Functions

    public IConversationContext StartConversation(string conversationId);
    public Task Delete(IConversationContext conversationContext);
}