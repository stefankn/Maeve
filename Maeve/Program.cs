using Maeve.Components;
using Maeve.Components.Database;
using OllamaSharp;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();

// Database
builder.Services.AddDbContextFactory<DataContext>();

// Ollama
builder.Services.AddTransient<IOllamaApiClient>(_ => {
    var host = Environment.GetEnvironmentVariable("OLLAMA_HOST") ?? "host.docker.internal";
    var port = Environment.GetEnvironmentVariable("OLLAMA_PORT") ?? "11434";
    return new OllamaApiClient($"http://{host}:{port}", "gemma3:12b");
});

// Blazor
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

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