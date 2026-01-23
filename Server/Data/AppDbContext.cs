using Microsoft.EntityFrameworkCore; 
using Server.Models; 

namespace Server.Data; 

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Song> Songs => Set<Song>(); 
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // add an index on the Language column for faster queries
        modelBuilder.Entity<Song>()
            .HasIndex(s => s.Language)
            .HasDatabaseName("IX_Songs_Language"); 
    }
} 
