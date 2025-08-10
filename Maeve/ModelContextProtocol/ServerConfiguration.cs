namespace Maeve.ModelContextProtocol;

public class ServerConfiguration {
    
    // - Properties
    
    public string? Key { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public string? Icon { get; set; }
    public required string Command { get; set; }
    public string[]? Args { get; set; }
}