using Server.Models.Music;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models.Game.Sessions;

public class ExternalPlaylistSession : BaseGameSession
{   
     // currently not implemented.
     // This will be used for a gamame that uses songs from a chosen playlist instead of the game songs.
    [NotMapped]
    public List<Song> SessionPlaylist { get; set; } = new();
    public string ExternalPlaylistId { get; set; } = string.Empty;
}