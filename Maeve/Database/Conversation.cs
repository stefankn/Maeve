using System.ComponentModel.DataAnnotations.Schema;
using Maeve.ModelProviders;

namespace Maeve.Database;

[Table("conversations")]
public class Conversation {
    
    // - Properties
    
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string? Id { get; init; }
    
    public required string Title { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; set; }
    public required Provider Provider { get; init; }
    public ICollection<Message> Messages { get; init; } = new List<Message>();
}