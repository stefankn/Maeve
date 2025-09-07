using System.Diagnostics;
using Maeve.Database;
using Microsoft.EntityFrameworkCore;

namespace DbMigrationService;

public class Worker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime
    ) : BackgroundService {
    
    // - Constants
    
    public const string ActivitySourceName = "DbMigrations";
    
    
    // - Private Properties

    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);
    
    
    // - Functions
    
    // BackgroundService Functions

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        using var activity = ActivitySource.StartActivity("Migrating database", ActivityKind.Client);

        try {
            await using var scope = serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
            
            await RunMigrationAsync(dbContext, stoppingToken);
        } catch (Exception ex) {
            activity?.AddException(ex);
            throw;
        }
        
        hostApplicationLifetime.StopApplication();
    }
    
    
    // - Private Functions

    private static async Task RunMigrationAsync(DataContext dbContext, CancellationToken stoppingToken) {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () => {
            await dbContext.Database.MigrateAsync(stoppingToken);
        });
    }
}