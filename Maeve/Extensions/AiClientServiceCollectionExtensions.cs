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

    public static IServiceCollection ConfigureAiClient(this IServiceCollection services, ConfigurationManager configuration) {
        var llmProvider = Environment.GetEnvironmentVariable("LLM_PROVIDER")?.AsProvider();

        return llmProvider switch {
            Provider.Ollama => services.SetupOllamaClient(),
            Provider.Antrophic => services.SetupAntrophicClient(configuration),
            _ => throw new Exception("LLM_PROVIDER environment variable is not set or unknown value")
        };
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
        
        services.AddSingleton<IModelProvider>(provider => {
            var keyValueStore = provider.GetRequiredService<IKeyValueStore>();
            return new OllamaModelProvider(ollamaClient, keyValueStore);
        });
                
        return services
            .AddDistributedMemoryCache()
            .AddSingleton(chatClient);
    }

    private static IServiceCollection SetupAntrophicClient(this IServiceCollection services, ConfigurationManager configuration) {
        using var factory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Trace));
        
        var client = new AnthropicClient(new APIAuthentication(configuration["Antrophic:ApiKey"]));
        var chatClient = client.Messages
            .AsBuilder()
            //.UseLogging(factory)
            .UseFunctionInvocation()
            .Build();
        services.AddSingleton<IModelProvider>(provider => {
            var keyValueStore = provider.GetRequiredService<IKeyValueStore>();
            return new AntrophicModelProvider(client, keyValueStore);
        });
                
        return services
            .AddDistributedMemoryCache()
            .AddSingleton(chatClient);
    }
}