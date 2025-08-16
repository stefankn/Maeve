using Anthropic.SDK;
using Maeve.Database.KeyValueStore;

namespace Maeve.ModelProviders.Antrophic;

public class AntrophicModelProvider(AnthropicClient client, IKeyValueStore keyValueStore): IModelProvider {
    
    // - Properties
    
    // IModelProvider Properties

    public string Name => "Antrophic";
    public Model[] AvailableModels { get; private set; } = [];

    public string? DefaultModelId {
        get => keyValueStore.GetString($"{Name}-defaultModel") ?? AvailableModels.FirstOrDefault()?.Id;
        set => keyValueStore.SetString(value, $"{Name}-defaultModel");
    }

    public int? MaxOutputTokens => 1000;


    // - Functions
    
    // IModelProvider Functions

    public async Task GetModelsAsync() {
        var modelList = await client.Models.ListModelsAsync();
        if (modelList == null) return;

        AvailableModels = modelList.Models.Select(m => new Model { Id = m.Id, Name = m.DisplayName }).ToArray();
    }
}