namespace Maeve.ModelContextProtocol;

public class ServerConfiguration {
    
    // - Properties
    
    public string? Key { get; set; }
    public string? Transport { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public string? Icon { get; set; }
    public string? Command { get; set; }
    public string[]? Args { get; set; }
    public string? Endpoint { get; set; }
}