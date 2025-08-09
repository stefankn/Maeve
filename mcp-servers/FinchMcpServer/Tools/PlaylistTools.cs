using System.ComponentModel;
using FinchMcpServer.APIClient;
using ModelContextProtocol.Server;

namespace FinchMcpServer.Tools;

[McpServerToolType]
internal class PlaylistTools(IApiClient apiClient) {
    
    // - Functions
    
    [McpServerTool(UseStructuredContent = true), Description("Returns a list of all available music playlists")]
    public async Task<Playlist[]> GetAllMusicPlaylists() {
        try {
            return await apiClient.GetAllMusicPlaylists();
        } catch {
            return [];
        }
    }
}