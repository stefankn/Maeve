using System.ComponentModel.DataAnnotations.Schema;

namespace Maeve.Database.KeyValueStore;

[Table("key_value_entries")]
public record KeyValueEntry {
    
    // - Properties
    
    [System.ComponentModel.DataAnnotations.Key]
    public string? Key { get; set; }
    
    public string? Value { get; set; }
}