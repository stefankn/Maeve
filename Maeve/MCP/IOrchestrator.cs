using McpClientTool = OllamaSharp.ModelContextProtocol.Server.McpClientTool;

namespace Maeve.MCP;

public interface IOrchestrator {
    
    // - Functions

    public Task<McpClientTool[]> GetTools();
}