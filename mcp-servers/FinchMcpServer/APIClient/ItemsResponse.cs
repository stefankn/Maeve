namespace FinchMcpServer.APIClient;

public class ItemsResponse<T> {
    
    // - Properties
    
    public required T[] Items { get; set; }
}