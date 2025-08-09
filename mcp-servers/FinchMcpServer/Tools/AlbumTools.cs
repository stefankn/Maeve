using System.ComponentModel;
using System.Text.Json;
using FinchMcpServer.APIClient;
using ModelContextProtocol.Server;

namespace FinchMcpServer.Tools;

[McpServerToolType]
internal class AlbumTools(IApiClient apiClient) {
    
    // - Functions
    
    [McpServerTool(UseStructuredContent = true), Description("Returns a list of all available music albums")]
    public async Task<Album[]> GetAllMusicAlbums() {
        try {
            return await apiClient.GetAllMusicAlbums();
        } catch {
            return [];
        }
    }
}