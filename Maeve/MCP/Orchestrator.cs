using OllamaSharp.ModelContextProtocol;
using McpClientTool = OllamaSharp.ModelContextProtocol.Server.McpClientTool;

namespace Maeve.MCP;

public class Orchestrator(IWebHostEnvironment environment): IOrchestrator {

    // - Functions

    public async Task<McpClientTool[]> GetTools() {
        var config = Path.Combine(environment.ContentRootPath, "mcp_server_config.json");
        return await Tools.GetFromMcpServers(config);
    }
}