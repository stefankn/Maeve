using Anthropic.SDK;
using Maeve.Database.KeyValueStore;
using Maeve.ModelProviders;
using Maeve.ModelProviders.Antrophic;
using Maeve.ModelProviders.Ollama;
using Microsoft.Extensions.AI;
using OllamaSharp;

namespace Maeve.Extensions;

public static class AiClientServiceCollectionExtensions {
    
    // - Functions

    public static IServiceCollection ConfigureAiClient(this IServiceCollection services) {
        return services
            .AddTransient<IModelProviderFactory, ModelProviderFactory>()
            .AddTransient<IChatClientFactory, ChatClientFactory>()
            .AddDistributedMemoryCache()
            .SetupOllamaClient()
            .SetupAntrophicClient();
    }

    private static IServiceCollection SetupOllamaClient(this IServiceCollection services) {
        using var factory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Trace));
        
        var host = Environment.GetEnvironmentVariable("OLLAMA_HOST") ?? "host.docker.internal";
        var port = Environment.GetEnvironmentVariable("OLLAMA_PORT") ?? "11434";
        var ollamaClient = new OllamaApiClient($"http://{host}:{port}");
        var chatClient = new ChatClientBuilder(ollamaClient)
            //.UseLogging(factory)
            .UseFunctionInvocation()
            .Build();

        services.AddKeyedSingleton<IModelProvider>(Provider.Ollama, (provider, _) => {
            var keyValueStore = provider.GetRequiredService<IKeyValueStore>();
            return new OllamaModelProvider(ollamaClient, keyValueStore);
        });

        return services.AddKeyedSingleton(Provider.Ollama, chatClient);
    }

    private static IServiceCollection SetupAntrophicClient(this IServiceCollection services) {
        using var factory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Trace));
        
        var client = new AnthropicClient(new APIAuthentication(Environment.GetEnvironmentVariable("ANTHROPHIC_API_KEY")));
        var chatClient = client.Messages
            .AsBuilder()
            //.UseLogging(factory)
            .UseFunctionInvocation()
            .Build();

        services.AddKeyedSingleton<IModelProvider>(Provider.Antrophic, (provider, _) => {
            var keyValueStore = provider.GetRequiredService<IKeyValueStore>();
            return new AntrophicModelProvider(client, keyValueStore);
        });
                
        return services.AddKeyedSingleton(Provider.Antrophic, chatClient);
    }
}