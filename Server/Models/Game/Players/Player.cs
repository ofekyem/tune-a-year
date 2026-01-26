using Server.Models.Game.Timeline;
using System.Text.Json.Serialization;

namespace Server.Models.Game.Players; 
[JsonDerivedType(typeof(Player), "Standard")]
[JsonDerivedType(typeof(OnlinePlayer), "Online")]
public class Player
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public int Tokens { get; set; } = 2; // start with 2 tokens
    public List<TimelineCard> Timeline { get; set; } = new(); 
    public bool HasWon => Timeline.Count >= 10; 
    public Guid BaseGameSessionId { get; set; }
    
}