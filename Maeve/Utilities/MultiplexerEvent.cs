namespace Maeve.Utilities;

public class MultiplexerEvent {
    
    // - Properties
    
    public required string Type { get; init; }
    public string? Content { get; init; }
}