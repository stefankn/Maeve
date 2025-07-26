namespace Maeve.Conversations;

using Message = Database.Message;

public interface IConversationContext {
    
    // - Properties

    public event EventHandler<string?>? Thoughts;
    public event EventHandler<string?>? ToolInvoked;
    public event EventHandler<Message>? NewMessage;
    public event EventHandler<string?>? Response;
    
    public Message[] Messages { get; }
    
    
    // - Functions

    public Task StartConversation();
    public Task SendMessage(string query);
}