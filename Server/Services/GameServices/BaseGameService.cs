using Server.Data;
using Server.Models.Game;
using Server.Models.Game.Sessions;
using Server.Models.Game.Timeline; 
using Server.Services.SongServices;
using Server.Services.Factories;
using Microsoft.EntityFrameworkCore;

namespace Server.Services.GameServices;

public abstract class BaseGameService : IGameService
{
    protected readonly AppDbContext _context;
    protected readonly SongServiceFactory _songServiceFactory;

    protected BaseGameService(AppDbContext context, SongServiceFactory songServiceFactory)
    {
        _context = context; 
        _songServiceFactory = songServiceFactory;
    }

    // verify if the placing in the timeline is correct
    public bool VerifyPlacement(List<TimelineCard> timeline, int newYear, int index)
    {
        if (index > 0 && timeline[index - 1].ReleaseYear > newYear) return false;
        if (index < timeline.Count && timeline[index].ReleaseYear < newYear) return false;
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

    // method that activates when the game requires to start
    public virtual async Task<BaseGameSession> StartGameAsync(Guid sessionId)
    {   
        // here we get the session including its players from the database
        var session = await _context.GameSessions
            .Include(s => s.Players)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null) throw new Exception("Game session not found.");
        if (session.Status != SessionStatus.Lobby) return session; // game already started

        // if there is no playlist url then its a local database songs service
        var songService = _songServiceFactory.GetService(session.Config.PlaylistUrl);

        // Fetch "anchor" cards for everyone 
        // we fetch for each player one random card 
        // there is an option to include 2 more arguments for minYear nad maxYear for filtering by year range (currently not used)
        var anchorSongs = await songService.GetRandomSongsAsync(
            session.Players.Count, 
            session.Config.Languages.ToArray(), 
            null, null, null);

        // each player gets one anchor card added to their timeline
        for (int i = 0; i < session.Players.Count; i++)
        {
            var song = anchorSongs[i];
            session.Players[i].Timeline.Add(new TimelineCard
            {
                SongId = song.Id,
                Title = song.Title,
                Artist = song.Artist,
                ReleaseYear = song.ReleaseYear,
            });
            // for the erf to recognize change in the timeline list
            session.Players[i].Timeline = session.Players[i].Timeline.ToList();
            session.PlayedSongIds.Add(song.Id.ToString());
        } 

        // for the erf to recognize change in the played song ids list
        session.PlayedSongIds = session.PlayedSongIds.ToList();

        // we get a pool of 10 random songs for the game
        var pool = await songService.GetRandomSongsAsync(
            10, 
            session.Config.Languages.ToArray(), 
            session.PlayedSongIds.ToArray(), null, null);

        // setting the first active song
        session.CurrentActiveSong = pool.First();
        session.PlayedSongIds.Add(session.CurrentActiveSong.Id.ToString());

        // update again for the erf after adding the active song
        session.PlayedSongIds = session.PlayedSongIds.ToList(); 
        
        // updating status and saving
        session.Status = SessionStatus.InProgress;
        await _context.SaveChangesAsync();

        return session;
    }
}