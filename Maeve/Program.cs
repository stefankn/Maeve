using Maeve;
using Maeve.Components;
using Maeve.Components.Database;
using Maeve.MCP;
using Maeve.Logging;
using OllamaSharp;
using ILogger = Maeve.Logging.ILogger;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();

// Database
builder.Services.AddDbContextFactory<DataContext>();

// Logging
builder.Logging.ClearProviders();
builder.Services.AddSingleton<ILogger>(provider => new Logger("Maeve", provider.GetRequiredService<IWebHostEnvironment>().IsDevelopment()));

builder.Services.AddTransient<IDocumentProcessor, DocumentProcessor>();

// Ollama
builder.Services.AddTransient<IOllamaApiClient>(_ => {
    var host = Environment.GetEnvironmentVariable("OLLAMA_HOST") ?? "host.docker.internal";
    var port = Environment.GetEnvironmentVariable("OLLAMA_PORT") ?? "11434";
    return new OllamaApiClient($"http://{host}:{port}", "qwen3:14b");
});

// Blazor
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<IOrchestrator, Orchestrator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app
    .MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();