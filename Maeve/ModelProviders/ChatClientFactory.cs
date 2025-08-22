using Maeve.Database.KeyValueStore;
using Microsoft.Extensions.AI;

namespace Maeve.ModelProviders;

public class ChatClientFactory(IServiceProvider serviceProvider): IChatClientFactory {
    
    // - Functions
    
    // IChatClientFactory Functions
    
    public IChatClient CreateChatClient(Provider provider) {
        return serviceProvider.GetRequiredKeyedService<IChatClient>(provider);
    }
}