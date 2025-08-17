using Maeve.Database;

namespace Maeve.Conversations;

public interface IConversationManager {
    
    // - Properties
    
    public Conversation[] Conversations { get; }
    public IConversationContext? FocusedConversation { get; }
    public event EventHandler<IConversationContext?>? OnConversationFocus;
    public event EventHandler? OnConversationUpdate;
    
    
    // - Functions

    public IConversationContext StartConversation(string conversationId);
    public void LeaveConversation(IConversationContext conversationContext);
    public Task Delete(IConversationContext conversationContext);
}