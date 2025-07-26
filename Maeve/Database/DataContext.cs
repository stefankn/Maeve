using Microsoft.EntityFrameworkCore;

namespace Maeve.Database;

public class DataContext: DbContext {
    
    // - Properties

    public DbSet<Message> Messages { get; set; } = null!;
    public DbSet<Document> Documents { get; set; } = null!;
    
    
    // - Functions
    
    // DbContext Functions

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.UseSqlite("Data Source=data/Maeve.db");
    }
}