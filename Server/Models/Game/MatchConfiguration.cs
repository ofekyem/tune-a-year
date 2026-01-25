
namespace Server.Models.Game;

// class for json of game settings we get from client when creating a game
public class MatchConfiguration
{
    // shared fields
    public GameMode Mode { get; set; }
    public MusicSource Source { get; set; } 
    public string? PlaylistUrl { get; set; }
    public List<string> Languages { get; set; } = new();
    public int MaxPlayers { get; set; }
    public int WinningScore { get; set; } = 10;

    // only for online game (in local this will be null)
    public string? CreatorName { get; set; }

    // only for local game (in online this will be null)
    public List<string>? LocalPlayerNames { get; set; }
}