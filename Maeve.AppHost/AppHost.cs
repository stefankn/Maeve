var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Maeve>("maeve");

builder
    .AddDockerfile("finch-mcp-server", "../mcp-servers/FinchMCPServer")
    .WithHttpEndpoint(port: 8018, targetPort: 8018);

builder
    .AddDockerfile("hue-mcp-server", "../mcp-servers/hue-mcp")
    .WithHttpEndpoint(port: 8023, targetPort: 8023)
    .WithBindMount("../mcp-servers/hue-mcp/config.json", "/root/.hue-mcp/config.json");

builder.Build().Run();
