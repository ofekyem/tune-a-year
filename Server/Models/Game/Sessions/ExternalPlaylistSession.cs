using Server.Models.Music;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models.Game.Sessions;

public class ExternalPlaylistSession : BaseGameSession
{   
     // this definition of not mapped is needed so i will handle it myself
    [NotMapped]
    public List<Song> SessionPlaylist { get; set; } = new();
    public string ExternalPlaylistId { get; set; } = string.Empty;
}