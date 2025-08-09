using System.ComponentModel;

namespace FinchMcpServer.APIClient;

public class Album {
    
    // - Properties
    
    [Description("The identifier of the album")]
    public required int Id { get; set; }
    
    [Description("The artist of the album")]
    public required string Artist { get; set; }
    
    [Description("The title of the album")]
    public required string Title { get; set; }
}