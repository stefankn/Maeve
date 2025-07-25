using System.Security.Cryptography;
using Maeve.Components.Database;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace Maeve;

public class DocumentProcessor(
    IDbContextFactory<DataContext> dbContextFactory,
    IWebHostEnvironment environment
    ): IDocumentProcessor {
    
    // - Functions

    public async Task<Document> Upload(IBrowserFile file) {
        var stream = new MemoryStream();
        await file.OpenReadStream(1024 * 1024 * 5).CopyToAsync(stream);

        var hash = await CalculateFileHash(stream);
        await EnsureUnique(hash);
        
        var filename = $"{Guid.NewGuid()}-{file.Name}";
        var path = Path.Combine(environment.WebRootPath, "documents", filename);
        await using FileStream fs = new(path, FileMode.Create);
        await stream.CopyToAsync(fs);
        
        return await CreateDocument(file.Name, filename, hash);
    }
    
    // - Private Functions
    
    private async Task<Document> CreateDocument(string name, string filename, string hash) {
        await using var dataContext = await dbContextFactory.CreateDbContextAsync();
        var document = new Document {
            Name = name,
            Filename = filename,
            Hash = hash,
            UploadedAt = DateTime.UtcNow
        };
        dataContext.Documents.Add(document);
        await dataContext.SaveChangesAsync();
        
        return document;
    }
    
    private async Task<string> CalculateFileHash(MemoryStream stream) {
        using var sha = SHA256.Create();
        await stream.FlushAsync();
        stream.Seek(0, SeekOrigin.Begin);
        var hashBytes = await sha.ComputeHashAsync(stream);
        stream.Seek(0, SeekOrigin.Begin);
        
        return Convert.ToHexString(hashBytes);
    }
    
    private async Task EnsureUnique(string hash) {
        await using var dataContext = await dbContextFactory.CreateDbContextAsync();
        if (await dataContext.Documents.AnyAsync(d => d.Hash == hash)) {
            throw new Exception("Document already exists");
        }
    }
}