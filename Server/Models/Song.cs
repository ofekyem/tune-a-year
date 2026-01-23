namespace Server.Models;

public class Song
{
    public Guid Id { get; set; }
    public String Title { get; set; } = string.Empty;
    public String Artist { get; set; } = string.Empty; 
    public int ReleaseYear { get; set; } 
    public String SpotifyId { get; set; } = string.Empty;
    public String PreviewUrl { get; set; } = string.Empty; 
    public String Language { get; set; } = "English";
}