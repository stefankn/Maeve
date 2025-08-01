using System.ComponentModel.DataAnnotations.Schema;

namespace Maeve.Database;

[Table("conversations")]
public class Conversation {
    
    // - Properties
    
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string? Id { get; init; }
    
    public required string Title { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; set; }
    public ICollection<Message> Messages { get; init; } = new List<Message>();
}