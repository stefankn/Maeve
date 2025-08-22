using Anthropic.SDK;
using Maeve.Database.KeyValueStore;
using Maeve.Extensions;

namespace Maeve.ModelProviders.Antrophic;

public class AntrophicModelProvider: IModelProvider {
    
    // - Private Properties

    private readonly AnthropicClient _client;
    private readonly IKeyValueStore _keyValueStore;
    
    
    // - Properties
    
    // IModelProvider Properties

    public Provider Provider => Provider.Antrophic;
    public Model[] AvailableModels { get; private set; } = [];
    public bool HasConfigurationError => AvailableModels.Length == 0;

    public string? DefaultModelId {
        get => _keyValueStore.GetString($"{Provider.GetDescriptionAttribute()}-defaultModel") ?? AvailableModels.FirstOrDefault()?.Id;
        set => _keyValueStore.SetString(value, $"{Provider.GetDescriptionAttribute()}-defaultModel");
    }

    public int? MaxOutputTokens => 8192;
    
    
    // - Construction

    public AntrophicModelProvider(AnthropicClient client, IKeyValueStore keyValueStore) {
        _client = client;
        _keyValueStore = keyValueStore;
        
        _ = Task.Run(async () => {
            await GetModelsAsync();
        });
    }


    // - Functions
    
    // IModelProvider Functions

    public async Task GetModelsAsync() {
        var modelList = await _client.Models.ListModelsAsync();
        if (modelList == null) return;

        AvailableModels = modelList.Models.Select(m => new Model { Id = m.Id, Name = m.DisplayName }).ToArray();
    }
}