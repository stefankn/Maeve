using Microsoft.Extensions.AI;

namespace Maeve.ModelProviders;

public interface IChatClientFactory {
    
    // - Functions
    
    public IChatClient CreateChatClient(Provider provider);
}