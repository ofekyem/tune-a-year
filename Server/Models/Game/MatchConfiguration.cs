using System.Text.Json.Serialization;

namespace Server.Models.Game;

// setting that will define the mode of the configs for online or local
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(LocalMatchConfiguration), 0)]  // 0 = Local
[JsonDerivedType(typeof(OnlineMatchConfiguration), 1)] // 1 = Online
public abstract class MatchConfiguration
{
    public GameMode Mode { get; set; }
    public MusicSource Source { get; set; } 
    public List<string> Languages { get; set; } = new();
    public int MaxPlayers { get; set; }
    public int WinningScore { get; set; } = 10;
}

// class for local match configurations
public class LocalMatchConfiguration : MatchConfiguration
{
    // you get all the names of the local players here
    public List<string> LocalPlayerNames { get; set; } = new();
}

// class for online match configurations
public class OnlineMatchConfiguration : MatchConfiguration
{
    // you get the room creator name here
    public string CreatorName { get; set; } = string.Empty;
}