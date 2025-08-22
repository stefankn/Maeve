using Maeve.Database.KeyValueStore;
using Maeve.Extensions;
using OllamaSharp;

namespace Maeve.ModelProviders.Ollama;

public class OllamaModelProvider: IModelProvider {
    
    // - Private Properties

    private readonly OllamaApiClient _client;
    private readonly IKeyValueStore _keyValueStore;
    

    // - Properties
    
    // IModelProvider Properties

    public Provider Provider => Provider.Ollama;
    public Model[] AvailableModels { get; private set; } = [];
    public bool HasConfigurationError => false;

    public string? DefaultModelId {
        get => _keyValueStore.GetString($"{Provider.GetDescriptionAttribute()}-defaultModel") ?? AvailableModels.FirstOrDefault()?.Id;
        set => _keyValueStore.SetString(value, $"{Provider.GetDescriptionAttribute()}-defaultModel");
    }

    public int? MaxOutputTokens => null;
    
    
    // - Construction

    public OllamaModelProvider(OllamaApiClient client, IKeyValueStore keyValueStore) {
        _client = client;
        _keyValueStore = keyValueStore;
        
        _ = Task.Run(async () => {
            await GetModelsAsync();
        });
    }
    
    
    // - Functions
    
    // IModelProvider Functions

    public async Task GetModelsAsync() {
        var models = await _client.ListLocalModelsAsync();

        // TODO: get model details to check abilities
        AvailableModels = models.Select(m => new Model { Id = m.Name, Name = m.Name }).ToArray();
    }
}