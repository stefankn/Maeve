using Microsoft.EntityFrameworkCore;

namespace Maeve.Components.Database;

public class DataContext: DbContext {
    
    // - Properties

    public DbSet<Message> Messages { get; set; } = null!;
    
    
    // - Functions
    
    // DbContext Functions

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.UseSqlite("Data Source=data/Maeve.db");
    }
}