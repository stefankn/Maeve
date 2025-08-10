using System.Text.Json;
using Maeve.Database.KeyValueStore;
using Maeve.Logging;
using OllamaSharp.ModelContextProtocol;
using OllamaSharp.ModelContextProtocol.Server;
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
    
    public void GetAvailableServers() {
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
        
        foreach (var serverConfiguration in configuration.Values) {
            Console.WriteLine($"Found configuration for {serverConfiguration.Name}");
        }

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
        AvailableTools = await Tools.GetFromMcpServers(EnabledServers.Select(s => new McpServerConfiguration {
            Name = s.Key,
            Command = s.Command,
            Arguments = s.Args
        }).ToArray());
        
        foreach (var tool in AvailableTools) {
            logger.Information($"Available tool: {tool.Function?.Name}, {tool.Function?.Description}", LogCategory.Tools, consoleLog: true);
        }
    }
}