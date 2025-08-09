using System.ComponentModel;

namespace FinchMcpServer.APIClient;

public class Playlist {
    
    // - Properties
    
    [Description("The identifier of the playlist")]
    public required int Id { get; set; }
    
    [Description("The title of the playlist")]
    public required string Name { get; set; }
}