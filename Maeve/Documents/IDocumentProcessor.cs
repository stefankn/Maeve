using DynamicData;
using Maeve.Database;
using Microsoft.AspNetCore.Components.Forms;

namespace Maeve.Documents;

public interface IDocumentProcessor {
    
    // - Functions

    public Task<Document> Upload(IBrowserFile file);
    public IObservable<Document?> Observe(Document document);
    public Task Delete(Document document);
}