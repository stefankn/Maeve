using Maeve.Components.Database;
using Microsoft.AspNetCore.Components.Forms;

namespace Maeve;

public interface IDocumentProcessor {
    
    // - Functions

    public Task<Document> Upload(IBrowserFile file);
}