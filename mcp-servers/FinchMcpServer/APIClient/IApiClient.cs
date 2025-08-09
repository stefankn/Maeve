namespace FinchMcpServer.APIClient;

public interface IApiClient {
    
    // - Functions

    public Task<Album[]> GetAllMusicAlbums();
    public Task<Playlist[]> GetAllMusicPlaylists();
}