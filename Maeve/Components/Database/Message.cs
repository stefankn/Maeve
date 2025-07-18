using System.ComponentModel.DataAnnotations.Schema;

namespace Maeve.Components.Database;

[Table("messages")]
public class Message {
    
    // - Properties
    
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string? Id { get; set; }
    
    public Role Role { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }
}