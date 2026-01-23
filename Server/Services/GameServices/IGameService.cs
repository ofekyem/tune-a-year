using Server.Models.Game.Sessions;
using Server.Models.Game;

namespace Server.Services.GameServices;

public interface IGameService
{
    Task<BaseGameSession> CreateGameAsync(MatchConfiguration config);
    string GenerateRoomCode();
    // next methods to be defined like JoinGame, SubmitGuess, UseToken
}