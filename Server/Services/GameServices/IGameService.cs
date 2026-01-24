using Server.Models.Game.Sessions;
using Server.Models.Game;
using Server.Models.Game.Timeline;

namespace Server.Services.GameServices;

public interface IGameService
{
    Task<BaseGameSession> CreateGameAsync(MatchConfiguration config);
    //Task StartGameAsync(Guid sessionId);
    bool VerifyPlacement(List<TimelineCard> timeline, int newYear, int index);
    
}