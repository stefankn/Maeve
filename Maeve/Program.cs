using Maeve.Components;
using Maeve.Conversations;
using Maeve.Database;
using Maeve.Database.KeyValueStore;
using Maeve.Documents;
using Maeve.Extensions;
using Maeve.Logging;
using Maeve.ModelContextProtocol;
using Maeve.ModelProviders;
using Microsoft.AspNetCore.StaticFiles;
using StackExchange.Redis;
using ILogger = Maeve.Logging.ILogger;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();

// Database
builder.Services.AddDbContextFactory<DataContext>();
builder.Services.AddSingleton<IKeyValueStore, KeyValueStore>();

// Redis
var connectionMultiplexer = ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("REDIS_HOST") ?? "redis");
builder.Services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);

// Logging
builder.Logging.ClearProviders();
builder.Services.AddSingleton<ILogger>(provider => new Logger("Maeve", provider.GetRequiredService<IWebHostEnvironment>().IsDevelopment()));

// Documents
builder.Services.AddTransient<IDocumentIngestClient, DocumentIngestClient>();
builder.Services.AddSingleton<IDocumentProcessor, DocumentProcessor>();

// MCP configuration
builder.Services.AddSingleton<IMcpConfigurator, McpConfigurator>();

// AI client
builder.Services.ConfigureAiClient(builder.Configuration);

builder.Services.AddSingleton<IConversationManager, ConversationManager>();

// Blazor
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var configurator = scope.ServiceProvider.GetRequiredService<IMcpConfigurator>();
configurator.UpdateAvailableServers();

var modelProvider = scope.ServiceProvider.GetRequiredService<IModelProvider>();
await modelProvider.GetModelsAsync();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".log"] = "text/plain";
app.UseStaticFiles(new StaticFileOptions {
    ContentTypeProvider = provider,
    ServeUnknownFileTypes = true
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app
    .MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();