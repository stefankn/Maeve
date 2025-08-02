using System.Security.Cryptography;
using System.Reactive.Linq;
using System.Text.Json;
using DynamicData;
using Maeve.Database;
using Maeve.Logging;
using Maeve.Utilities;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using ILogger = Maeve.Logging.ILogger;

namespace Maeve.Documents;

public class DocumentProcessor(
    IDbContextFactory<DataContext> dbContextFactory,
    IWebHostEnvironment environment,
    IConnectionMultiplexer connectionMultiplexer,
    IDocumentIngestClient documentIngestClient,
    ILogger logger
    ): IDocumentProcessor {
    
    // - Private Properties

    private readonly Dictionary<string, ChannelMessageQueue> _channels = [];
    private readonly SourceCache<Document, string> _documents = new (d => d.Hash);
    
    
    // - Functions

    public async Task<Document> Upload(IBrowserFile file) {
        var stream = new MemoryStream();
        await file.OpenReadStream(1024 * 1024 * 5).CopyToAsync(stream);

        var hash = await CalculateFileHash(stream);
        await EnsureUnique(hash);
        
        var id = Guid.NewGuid().ToString();
        var filename = $"{id.Split("-")[0]}-{file.Name}";
        var path = Path.Combine(environment.WebRootPath, "documents", filename);
        await using FileStream fs = new(path, FileMode.Create);
        await stream.CopyToAsync(fs);
        
        var document = await CreateDocument(file.Name, filename, hash);
        _documents.AddOrUpdate(document);

        await MonitorState(document);
        await documentIngestClient.Ingest(filename, hash);
        
        return document;
    }

    public IObservable<Document?> Observe(Document document) {
        return _documents
            .Connect()
            .Select(c => c.FirstOrDefault(d => d.Key == document.Hash).Current);
    }

    public async Task Delete(Document document) {
        try {
            await documentIngestClient.Delete(document);
        } catch (Exception e) {
            logger.Error("Failed to delete vectors for document", LogCategory.Documents, consoleLog: true);
            logger.Error(e.ToString(), LogCategory.Documents);
        }
    }


    // - Private Functions
    
    private async Task<Document> CreateDocument(string name, string filename, string hash) {
        await using var dataContext = await dbContextFactory.CreateDbContextAsync();
        var document = new Document {
            Name = name,
            Filename = filename,
            Hash = hash,
            UploadedAt = DateTime.UtcNow,
            State = DocumentState.Uploading
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

    private async Task MonitorState(Document document) {
        if (_channels.ContainsKey(document.Hash)) return;
        
        var subscriber = connectionMultiplexer.GetSubscriber();
        var channel = await subscriber.SubscribeAsync(RedisChannel.Literal(document.Hash));
        _channels[document.Hash] = channel;
        
        channel.OnMessage(message => {
            try {
                var processingMessage = JsonSerializer.Deserialize<MultiplexerEvent>(message.Message.ToString(), new JsonSerializerOptions {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                if (processingMessage is not { Type: "ingest" }) return;
                    
                var state = processingMessage.Content switch {
                    "processing" => DocumentState.Processing,
                    "vectorizing" => DocumentState.Vectorizing,
                    "completed" => DocumentState.Vectorized,
                    "error" => DocumentState.Failed,
                    _ => DocumentState.Uploading
                };

                if (state == DocumentState.Uploading || state == document.State) return;
                
                using var dataContext = dbContextFactory.CreateDbContext();
                var doc = dataContext.Documents.Find(document.Id);
                if (doc != null) {
                    doc.State = state;
                    dataContext.SaveChanges();
                    
                    _documents.AddOrUpdate(doc);
                }
                
                if (state == DocumentState.Vectorized) {
                    _channels[document.Hash].Unsubscribe();
                    _channels.Remove(document.Hash);
                }
            } catch (Exception e) {
                logger.Error("Failed to process document update",  LogCategory.Documents, consoleLog: true);
                logger.Error(e.ToString(), LogCategory.Documents);
            }
        });

    }
}