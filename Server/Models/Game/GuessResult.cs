namespace Server.Models.Game;

public class GuessResult
{
    public bool PlacementCorrect { get; set; }
    public bool BonusEarned { get; set; }
    public string CorrectTitle { get; set; } = string.Empty;
    public string CorrectArtist { get; set; } = string.Empty;
    public int CorrectYear { get; set; }
    
    // the id of the player that made the guess
    public Guid PlayerId { get; set; }
}