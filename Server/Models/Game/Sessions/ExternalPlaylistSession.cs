using Server.Models.Music;

namespace Server.Models.Game.Sessions;

public class ExternalPlaylistSession : BaseGameSession
{
    // Temporary list of songs fetched from the external API
    public List<Song> SessionPlaylist { get; set; } = new();
    public string ExternalPlaylistId { get; set; } = string.Empty;
}