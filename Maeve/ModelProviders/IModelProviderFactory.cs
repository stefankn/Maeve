namespace Maeve.ModelProviders;

public interface IModelProviderFactory {
    
    // - Functions

    public IModelProvider? CreateDefaultModelProvider();
    public IModelProvider CreateModelProvider(Provider provider);
}