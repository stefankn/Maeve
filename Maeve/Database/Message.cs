using System.ComponentModel.DataAnnotations.Schema;

namespace Maeve.Database;

[Table("messages")]
public class Message {
    
    // - Properties
    
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string? Id { get; init; }
    
    public Role Role { get; init; }
    public required string Content { get; init; }
    public string? Thoughts { get; init; }
    public List<Tool> Tools { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    
    public Conversation Conversation { get; set; } = null!;
    public string ConversationId { get; set; } = null!;
}