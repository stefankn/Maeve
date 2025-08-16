namespace Maeve.ModelProviders;

public interface IModelProvider {
    
    // - Properties
    
    public Provider Provider { get; }
    public Model[] AvailableModels { get; }
    public string? DefaultModelId { get; set; }
    public int? MaxOutputTokens { get; }
    public bool HasConfigurationError { get; }
    
    
    // - Functions

    public Task GetModelsAsync();
}