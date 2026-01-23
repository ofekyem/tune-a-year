using Server.Data;
using Server.Models.Game;
using Server.Models.Game.Sessions;
using Server.Models.Game.Timeline;
using Microsoft.EntityFrameworkCore;

namespace Server.Services.GameServices;

public abstract class BaseGameService : IGameService
{
    protected readonly AppDbContext _context;

    protected BaseGameService(AppDbContext context)
    {
        _context = context;
    }

    // verify if the placing in the timeline is correct
    public bool VerifyPlacement(List<TimelineCard> timeline, int newYear, int index)
    {
        if (index > 0 && timeline[index - 1].Year > newYear) return false;
        if (index < timeline.Count && timeline[index].Year < newYear) return false;
        return true;
    } 

    // common initialization logic for game sessions
    protected BaseGameSession InitializeSession(MatchConfiguration config)
    {
        BaseGameSession session = config.Source == MusicSource.LocalDatabase 
            ? new LocalDatabaseSession() 
            : new ExternalPlaylistSession();

        session.Config = config;
        session.Status = SessionStatus.Lobby;
        session.CreatedAt = DateTime.UtcNow;
        return session;
    }

    // abstract function - each subclass must implement it in its own way
    public abstract Task<BaseGameSession> CreateGameAsync(MatchConfiguration config);

    // virtual function - default implementation that can be overridden if needed
    // public virtual async Task StartGameAsync(Guid sessionId)
    // {
    //     // general logic to start a game, deal cards, etc.
    // }
}