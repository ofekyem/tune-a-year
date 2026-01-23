using Server.Data;
using Server.Models.Game;
using Server.Models.Game.Sessions;
using Server.Models.Game.Players;  
using Server.Models.Game.Timeline;
using Microsoft.EntityFrameworkCore; 

namespace Server.Services.GameServices;
public class LocalGameService : BaseGameService
{
    public LocalGameService(AppDbContext context) : base(context) { }

    public override async Task<BaseGameSession> CreateGameAsync(MatchConfiguration config)
    {   
        var localConfig = (LocalMatchConfiguration)config; 

        if (localConfig.LocalPlayerNames == null || localConfig.LocalPlayerNames.Count < 2)
        {
            throw new Exception("At least two local player name is required for local games.");
        }

        var session = InitializeSession(localConfig); 

        _context.GameSessions.Add(session);

        // create object for each of the players of the game
        foreach (var name in localConfig.LocalPlayerNames)
        {
            var player = new Player // use base Player class
            {
                Name = name,
                Tokens = 2,
                Timeline = new List<TimelineCard>(),
                BaseGameSessionId = session.Id 
            };

            _context.Players.Add(player);
        }
        
        await _context.SaveChangesAsync();
        return session;
    }
}