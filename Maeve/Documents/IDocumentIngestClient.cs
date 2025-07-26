using Maeve.Database;

namespace Maeve.Documents;

public interface IDocumentIngestClient {
    
    // - Functions
    
    public Task Ingest(string filename, string hash);
    public Task Delete(Document document);
}