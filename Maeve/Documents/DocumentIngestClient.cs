using System.Text;
using System.Text.Json;
using Maeve.Database;

namespace Maeve.Documents;

public class DocumentIngestClient(IHttpClientFactory httpClientFactory): IDocumentIngestClient {
    
    // - Private Properties

    private string Host => Environment.GetEnvironmentVariable("RAG_INGESTER_HOST") ?? "rag-ingester";
    
    
    // - Functions
    
    // IDocumentIngestClient Functions
    
    public async Task Ingest(string filename, string hash) {
        var request = new HttpRequestMessage(HttpMethod.Post, $"http://{Host}/ingest");

        var json = JsonSerializer.Serialize(new { document = filename, file_hash = hash });
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var client = httpClientFactory.CreateClient();
        await client.SendAsync(request);
    }

    public async Task Delete(Document document) {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"http://{Host}/document");
        var json = JsonSerializer.Serialize(new { file_hash = document.Hash });
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var client = httpClientFactory.CreateClient();
        await client.SendAsync(request);
    }
}