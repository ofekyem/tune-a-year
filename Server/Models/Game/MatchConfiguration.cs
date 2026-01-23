namespace Server.Models.Game;

public class MatchConfiguration
{
    public GameMode Mode { get; set; }
    public MusicSource Source { get; set; }
    public List<string> Languages { get; set; } = new();
    public int MaxPlayers { get; set; }
    public int WinningScore { get; set; } = 10;
}