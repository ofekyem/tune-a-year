using Microsoft.EntityFrameworkCore; 
using Server.Models.Music; 
using Server.Models.Game;
using Server.Models.Game.Sessions;
using Server.Models.Game.Players;

namespace Server.Data; 

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Song> Songs { get; set; }
    public DbSet<BaseGameSession> GameSessions { get; set; }
    public DbSet<Player> Players { get; set; } 
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // add an index on the Language column for faster queries
        modelBuilder.Entity<Song>()
            .HasIndex(s => s.Language)
            .HasDatabaseName("IX_Songs_Language"); 

        // Configure MatchConfiguration to be stored as JSON
        modelBuilder.Entity<BaseGameSession>()
        .Property(b => b.Config)
        .HasColumnType("jsonb"); 
        
        // Configure TPH for GameSession inheritance
        modelBuilder.Entity<BaseGameSession>()
            .HasDiscriminator<string>("SessionType")
            .HasValue<LocalDatabaseSession>("Local")
            .HasValue<ExternalPlaylistSession>("External"); 

        // Configure Session with playlist version to be stored as JSON
        modelBuilder.Entity<ExternalPlaylistSession>()
            .Property(s => s.SessionPlaylist)
            .HasColumnType("jsonb"); 

        // Configure TPH for Player inheritance
        modelBuilder.Entity<Player>()
            .HasDiscriminator<string>("PlayerType")
            .HasValue<Player>("Standard")
            .HasValue<OnlinePlayer>("Online");
        
        // Configure Timeline to be stored as JSON
        modelBuilder.Entity<Player>()
            .Property(p => p.Timeline)
            .HasColumnType("jsonb");
        

    }
} 
