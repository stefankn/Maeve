using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Maeve.Components.Database;

[Table("documents")]
[Index(nameof(Hash), IsUnique = true)]
public class Document {
    
    // - Properties
    
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string? Id { get; init; }
    
    public required string Name { get; init; } = null!;
    public required string Filename { get; init; } = null!;
    public required string Hash { get; init; } = null!;
    public DateTime UploadedAt { get; init; }
}