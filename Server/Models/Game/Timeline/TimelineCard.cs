namespace Server.Models.Game.Timeline;

public class TimelineCard
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SongId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public int Year { get; set; }
}

