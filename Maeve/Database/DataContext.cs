using Microsoft.EntityFrameworkCore;

namespace Maeve.Database;

public class DataContext: DbContext {
    
    // - Properties

    public DbSet<Conversation> Conversations { get; set; } = null!;
    public DbSet<Document> Documents { get; set; } = null!;
    
    
    // - Functions
    
    // DbContext Functions

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.UseSqlite("Data Source=data/Maeve.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Message>().OwnsMany(message => message.Tools, builder => {
            builder.ToJson();
            builder.OwnsMany(tool => tool.Arguments);
        });
    }
}