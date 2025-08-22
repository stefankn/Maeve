using Maeve.Database.KeyValueStore;

namespace Maeve.ModelProviders;

public class ModelProviderFactory(
    IServiceProvider serviceProvider,
    IKeyValueStore keyValueStore
    ): IModelProviderFactory {
    
    // - Functions
    
    // IModelProviderFactory Functions

    public IModelProvider? CreateDefaultModelProvider() {
        var defaultProvider = keyValueStore.GetEnum<Provider>("DefaultLLMProvider");
        return serviceProvider.GetKeyedService<IModelProvider>(defaultProvider);
    }

    public IModelProvider CreateModelProvider(Provider provider) {
        return serviceProvider.GetRequiredKeyedService<IModelProvider>(provider);
    }
}