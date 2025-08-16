namespace Maeve.ModelProviders;

public interface IModelProvider {
    
    // - Properties
    
    public string Name { get; }
    public Model[] AvailableModels { get; }
    public string? DefaultModelId { get; set; }
    public int? MaxOutputTokens { get; }
    
    
    // - Functions

    public Task GetModelsAsync();
}