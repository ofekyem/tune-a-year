using Server.Models.Music;
using Server.Models.Game;
using Server.Models.Game.Players; 

namespace Server.Models.Game.Sessions;

public abstract class BaseGameSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string RoomCode { get; set; } = string.Empty;
    public SessionStatus Status { get; set; } = SessionStatus.Lobby;
    public MatchConfiguration Config { get; set; } = new();
    
    public List<Player> Players { get; set; } = new();
    public int CurrentPlayerIndex { get; set; } = 0;
    public List<string> PlayedSongIds { get; set; } = new();
    
    // The song that is currently playing and waiting for a guess
    public Song? CurrentActiveSong { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}