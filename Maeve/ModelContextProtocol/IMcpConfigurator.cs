using ModelContextProtocol.Client;

namespace Maeve.ModelContextProtocol;

public interface IMcpConfigurator {
    
    // - Properties
    
    public ServerConfiguration[] AvailableServers { get; }
    public ServerConfiguration[] EnabledServers { get; }
    public McpClientTool[] AvailableTools { get; }
    
    
    // - Functions

    public void UpdateAvailableServers();
    public bool IsEnabled(ServerConfiguration server);
    public void ToggleServer(ServerConfiguration server, bool isEnabled);
}