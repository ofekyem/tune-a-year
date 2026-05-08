using Server.Data;
using Server.Models.Game;
using Server.Models.Game.Sessions;
using Server.Models.Game.Players;  
using Server.Models.Game.Timeline;
using Server.Services.SongServices;
using Server.Services.Factories;
using Microsoft.EntityFrameworkCore; 

namespace Server.Services.GameServices;
public class LocalGameService : BaseGameService
{
    public LocalGameService(AppDbContext context, SongServiceFactory songServiceFactory) : base(context, songServiceFactory) { }

    public override async Task<BaseGameSession> CreateGameAsync(MatchConfiguration config)
    {   

        if (config.LocalPlayerNames == null || config.LocalPlayerNames.Count < 2)
        {
            throw new Exception("At least two local player name is required for local games.");
        }

        // Initialize the game session with the provided configuration
        var session = InitializeSession(config); 

        // Add the game session to the database context
        _context.GameSessions.Add(session);

        // create object for each of the players of the game
        int order = 0;
        foreach (var name in config.LocalPlayerNames)
        {
            var player = new Player // use base Player class
            {
                Name = name,
                Tokens = 2,
                Timeline = new List<TimelineCard>(),
                BaseGameSessionId = session.Id,
                JoinOrder = order++
            };

            // add each player to database context
            _context.Players.Add(player);
        }
        
        // save the context to the database, which will generate IDs for the session and players.
        await _context.SaveChangesAsync();

        // Becuase its Offline mode, we can start the game immediately after creation.
        return await StartGameAsync(session.Id);
    }
}