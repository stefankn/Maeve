using System.Text.Json;

namespace Maeve.Database;

public class Tool {
    
    // - Types

    public class Argument {
        public required string Key { get; set; }
        public required string? Value { get; set; }
    }
    
    
    // - Properties
    
    public required string Function { get; set; }
    public List<Argument> Arguments { get; init; } = [];

    public string ArgumentsDescription => JsonSerializer.Serialize(Arguments);
    public string Description => $"{Function}, arguments: {ArgumentsDescription}";
}