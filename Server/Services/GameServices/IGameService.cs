using Server.Models.Game.Sessions;
using Server.Models.Game;
using Server.Models.Game.Timeline;

namespace Server.Services.GameServices;

public interface IGameService
{
    Task<BaseGameSession> CreateGameAsync(MatchConfiguration config);
    bool VerifyPlacement(List<TimelineCard> timeline, int newYear, int index); 

    Task<BaseGameSession> StartGameAsync(Guid sessionId);
    
    Task<(BaseGameSession session, GuessResult result)> SubmitGuessAsync(
        Guid sessionId, 
        Guid playerId, 
        int targetIndex, 
        string? titleGuess, 
        string? artistGuess);
    
}