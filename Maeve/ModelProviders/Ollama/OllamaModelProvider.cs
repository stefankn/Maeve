using Maeve.Database.KeyValueStore;
using Maeve.Extensions;
using OllamaSharp;

namespace Maeve.ModelProviders.Ollama;

public class OllamaModelProvider(OllamaApiClient client, IKeyValueStore keyValueStore): IModelProvider {

    // - Properties
    
    // IModelProvider Properties

    public Provider Provider => Provider.Ollama;
    public Model[] AvailableModels { get; private set; } = [];
    public bool HasConfigurationError => false;

    public string? DefaultModelId {
        get => keyValueStore.GetString($"{Provider.GetDescriptionAttribute()}-defaultModel") ?? AvailableModels.FirstOrDefault()?.Id;
        set => keyValueStore.SetString(value, $"{Provider.GetDescriptionAttribute()}-defaultModel");
    }

    public int? MaxOutputTokens => null;
    
    
    // - Functions
    
    // IModelProvider Functions

    public async Task GetModelsAsync() {
        var models = await client.ListLocalModelsAsync();

        // TODO: get model details to check abilities
        AvailableModels = models.Select(m => new Model { Id = m.Name, Name = m.Name }).ToArray();
    }
}