using Maeve.Database.KeyValueStore;
using Microsoft.Extensions.AI;

namespace Maeve.ModelProviders;

public class ChatClientFactory(
    IServiceProvider serviceProvider,
    IKeyValueStore keyValueStore): IChatClientFactory {
    
    // - Functions
    
    // IChatClientFactory Functions
    
    public IChatClient? CreateChatClient(Provider? provider = null) {
        if (provider == null) {
            var defaultProvider = keyValueStore.GetEnum<Provider>("DefaultLLMProvider");
            provider = defaultProvider;
        }
        
        return serviceProvider.GetKeyedService<IChatClient>(provider);
    }
}