using System.Net.Http.Json;
using System.Text.Json;

namespace FinchMcpServer.APIClient;

internal class ApiClient(IHttpClientFactory httpClientFactory): IApiClient {
    
    // - Functions

    public async Task<Album[]> GetAllMusicAlbums() {
        using var httpClient = httpClientFactory.CreateClient();
        
        httpClient.BaseAddress = new Uri("http://192.168.0.250:25520");
        var response = await httpClient.GetAsync("/api/v1/albums?per=1000");
        response.EnsureSuccessStatusCode();
        
        var contentString = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        };
        var content = JsonSerializer.Deserialize<ItemsResponse<Album>>(contentString, options);
        
        return content?.Items ?? throw new Exception("Invalid response");
    }
    
    public async Task<Playlist[]> GetAllMusicPlaylists() {
        using var httpClient = httpClientFactory.CreateClient();
        
        httpClient.BaseAddress = new Uri("http://192.168.0.250:25520");
        var response = await httpClient.GetAsync("/api/v1/playlists");
        response.EnsureSuccessStatusCode();
        
        var contentString = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        };
        var playlists = JsonSerializer.Deserialize<Playlist[]>(contentString, options);
        
        return playlists ?? throw new Exception("Invalid response");
    }
}