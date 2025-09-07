using Maeve.Database.KeyValueStore;
using Microsoft.EntityFrameworkCore;

namespace Maeve.Database;

public class DataContext(DbContextOptions<DataContext> options): DbContext(options) {
    
    // - Properties

    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<KeyValueEntry> KeyValueEntries => Set<KeyValueEntry>();
    
    
    // - Functions
    
    // DbContext Functions

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Message>().OwnsMany(message => message.Tools, builder => {
            builder.ToJson();
            builder.OwnsMany(tool => tool.Arguments);
        });
    }
}