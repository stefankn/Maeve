using Maeve.Database;

namespace Maeve.Conversations;

using Message = Database.Message;

public interface IConversationContext {
    
    // - Properties

    public event EventHandler<string?>? OnThoughts;
    public event EventHandler<Tool?>? OnToolInvocation;
    public event EventHandler<Message>? OnNewMessage;
    public event EventHandler<string?>? OnResponse;
    
    public string? Id { get; }
    public string Title { get; }
    public bool IsResponding { get; }
    public Message[] Messages { get; }
    public string? Response { get; }
    public string? Thoughts { get; }
    public Tool[] UsedTools { get; }
    
    
    // - Functions
    
    public Task SendMessage(string query);
}