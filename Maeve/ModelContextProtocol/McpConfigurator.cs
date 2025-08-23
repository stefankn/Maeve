using System.Text.Json;
using Maeve.Database.KeyValueStore;
using Maeve.Logging;
using ModelContextProtocol.Client;
using ILogger = Maeve.Logging.ILogger;

namespace Maeve.ModelContextProtocol;

public class McpConfigurator(
    IWebHostEnvironment environment,
    IKeyValueStore keyValueStore,
    ILogger logger
    ): IMcpConfigurator {
    
    // - Properties

    public ServerConfiguration[] AvailableServers { get; private set; } = [];
    public ServerConfiguration[] EnabledServers { get; private set; } = [];
    public McpClientTool[] AvailableTools { get; private set; } = [];
    

    // - Functions
    
    public void UpdateAvailableServers() {
        var path = Path.Combine(environment.ContentRootPath, "mcp_server_config.json");
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);

        var options = new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
        var configuration = JsonSerializer
            .Deserialize<Dictionary<string, Dictionary<string, ServerConfiguration>>>(stream, options)?
            .FirstOrDefault()
            .Value ?? [];

        AvailableServers = configuration.Select(pair => {
            var server = pair.Value;
            server.Key = pair.Key;
            return server;
        }).ToArray();

        UpdateEnabledServers();
    }

    public bool IsEnabled(ServerConfiguration server) {
        return EnabledServers.Any(s => s.Key == server.Key);
    }

    public void ToggleServer(ServerConfiguration server, bool isEnabled) {
        if (server.Key == null) return;
        
        keyValueStore.SetBool(isEnabled, server.Key);
        UpdateEnabledServers();
    }
    
    
    // - Private Functions

    private void UpdateEnabledServers() {
        EnabledServers = AvailableServers.Where(s => s.Key != null && keyValueStore.GetBool(s.Key)).ToArray();
        
        _ = Task.Run(async () => await UpdateAvailableTools());
    }

    private async Task UpdateAvailableTools() {
        try {
            var availableTools = new List<McpClientTool>();
            foreach (var serverConfiguration in EnabledServers) {
                var transportOptions = new StdioClientTransportOptions {
                    Name = serverConfiguration.Name,
                    Command = serverConfiguration.Command,
                    Arguments = serverConfiguration.Args
                };
            
                var client = await McpClientFactory.CreateAsync(new StdioClientTransport(transportOptions));
                availableTools.AddRange(await client.ListToolsAsync());
            }
        
            AvailableTools = availableTools.ToArray();
        
            foreach (var tool in AvailableTools) {
                logger.Information($"Available tool: {tool.Name}, {tool.Description}", LogCategory.Tools, consoleLog: true);
            }
        } catch (Exception e) {
            logger.Error($"Failed to retrieve tools, {e}", LogCategory.Tools, true);
        }
    }
}