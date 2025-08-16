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
        using var factory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Trace));

        // Ollama
        var host = Environment.GetEnvironmentVariable("OLLAMA_HOST") ?? "host.docker.internal";
        var port = Environment.GetEnvironmentVariable("OLLAMA_PORT") ?? "11434";
        var client = new OllamaApiClient($"http://{host}:{port}");
        var chatClient = new ChatClientBuilder(client)
            //.UseLogging(factory)
            .UseFunctionInvocation()
            .Build();
        
        services.AddSingleton<IModelProvider>(provider => {
            var keyValueStore = provider.GetRequiredService<IKeyValueStore>();
            return new OllamaModelProvider(client, keyValueStore);
        });
        
        // Claude
        // var client = new AnthropicClient(new APIAuthentication(configuration["Antrophic:ApiKey"]));
        // var chatClient = client.Messages
        //     .AsBuilder()
        //     .UseLogging(factory)
        //     .UseFunctionInvocation()
        //     .Build();
        // services.AddSingleton<IModelProvider>(provider => {
        //     var keyValueStore = provider.GetRequiredService<IKeyValueStore>();
        //     return new AntrophicModelProvider(client, keyValueStore);
        // });

        return services
            .AddDistributedMemoryCache()
            .AddSingleton(chatClient);
    }
}