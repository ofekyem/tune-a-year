namespace Server.Models.Game.Players;

public class OnlinePlayer : Player
{
    // Unique identifier for the connection in SignalR (real-time communication)
    public string? ConnectionId { get; set; }
    
    // Is this player the host of the room?
    public bool IsHost { get; set; } = false;
    
    // Is the player currently connected? (helps to track disconnections)
    public bool IsConnected { get; set; } = true; 
    
}