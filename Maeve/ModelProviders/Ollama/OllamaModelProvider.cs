using Maeve.Database.KeyValueStore;
using OllamaSharp;

namespace Maeve.ModelProviders.Ollama;

public class OllamaModelProvider(OllamaApiClient client, IKeyValueStore keyValueStore): IModelProvider {

    // - Properties
    
    // IModelProvider Properties

    public string Name => "Ollama";
    public Model[] AvailableModels { get; private set; } = [];
    
    public string? DefaultModelId {
        get => keyValueStore.GetString($"{Name}-defaultModel") ?? AvailableModels.FirstOrDefault()?.Id;
        set => keyValueStore.SetString(value, $"{Name}-defaultModel");
    }

    public int? MaxOutputTokens => null;
    
    
    // - Functions
    
    // IModelProvider Functions

    public async Task GetModelsAsync() {
        var models = await client.ListLocalModelsAsync();

        AvailableModels = models.Select(m => new Model { Id = m.Name, Name = m.Name }).ToArray();
    }
}