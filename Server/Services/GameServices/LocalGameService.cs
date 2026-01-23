using Server.Data;
using Server.Models.Game;
using Server.Models.Game.Sessions;
using Microsoft.EntityFrameworkCore; 

namespace Server.Services.GameServices;
public class LocalGameService : BaseGameService
{
    public LocalGameService(AppDbContext context) : base(context) { }

    public override async Task<BaseGameSession> CreateGameAsync(MatchConfiguration config)
    {
        var session = InitializeSession(config); 
        
        _context.GameSessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }
}