using Anthropic.SDK;
using Maeve.Database.KeyValueStore;
using Maeve.Extensions;

namespace Maeve.ModelProviders.Antrophic;

public class AntrophicModelProvider(AnthropicClient client, IKeyValueStore keyValueStore) : IModelProvider {
    
    // - Properties
    
    // IModelProvider Properties

    public Provider Provider => Provider.Antrophic;
    public Model[] AvailableModels { get; private set; } = [];
    public bool HasConfigurationError => AvailableModels.Length == 0;

    public string? DefaultModelId {
        get => keyValueStore.GetString($"{Provider.GetDescriptionAttribute()}-defaultModel") ?? AvailableModels.FirstOrDefault()?.Id;
        set => keyValueStore.SetString(value, $"{Provider.GetDescriptionAttribute()}-defaultModel");
    }

    public int? MaxOutputTokens => 8192;

 
    // - Functions
    
    // IModelProvider Functions

    public async Task GetModelsAsync() {
        var modelList = await client.Models.ListModelsAsync();
        if (modelList == null) return;

        AvailableModels = modelList.Models.Select(m => new Model { Id = m.Id, Name = m.DisplayName }).ToArray();
    }
}