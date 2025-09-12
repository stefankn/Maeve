DotNetEnv.Env.Load();

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false)
    .WithPgWeb(pgWeb => pgWeb.WithHostPort(5050));

if (builder.ExecutionContext.IsPublishMode) {
    postgres.WithEndpoint(isProxied: false);
}

var postgresDb = postgres.AddDatabase("maeve-db");

var migrations = builder.AddProject<Projects.DbMigrationService>("migrations")
    .WithReference(postgresDb)
    .WaitFor(postgresDb);

var finchMcpServer = builder
    .AddDockerfile("finch-mcp-server", "../mcp-servers/FinchMCPServer");

var hueMcpServer = builder
    .AddDockerfile("hue-mcp-server", "../mcp-servers/hue-mcp");

if (builder.ExecutionContext.IsRunMode) {
    finchMcpServer.WithHttpEndpoint(port: 8018, targetPort: 8018);
    hueMcpServer.WithHttpEndpoint(port: 8023, targetPort: 8023);
}

var maeve = builder.AddProject<Projects.Maeve>("maeve")
    .WithReference(postgresDb)
    .WithReference(migrations)
    .WaitForCompletion(migrations)
    .WaitFor(finchMcpServer)
    .WaitFor(hueMcpServer);

if (builder.ExecutionContext.IsRunMode) {
    maeve
        .WithEnvironment("MCP_SERVERS_CONFIG_FILE", "mcp-server-config.local.json")
        .WithEnvironment("OLLAMA_HOST", Environment.GetEnvironmentVariable("OLLAMA_HOST"));
} else {
    maeve.WithEnvironment("ANTHROPHIC_API_KEY", Environment.GetEnvironmentVariable("ANTHROPHIC_API_KEY"));
}

builder.Build().Run();
