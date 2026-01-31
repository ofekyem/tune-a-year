using Server.Data;
using Server.Models.Game;
using Server.Models.Game.Players;
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

    // method to get a game session and sort players deterministically
    protected async Task<BaseGameSession?> GetSessionWithSortedPlayersAsync(Guid sessionId)
    {
        var session = await _context.GameSessions
            .Include(s => s.Players)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session != null && session.Players != null)
        {
            // sort players by JoinOrder
            session.Players = session.Players
                .OrderBy(p => p.JoinOrder)
                .ToList();
        }

        return session;
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
        // decide which subclass of BaseGameSession to create
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
        var session = await GetSessionWithSortedPlayersAsync(sessionId);

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

        // we get a first active song for the game
        var pool = await songService.GetRandomSongsAsync(
            1, 
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

    
    // method that processes a player's guess submission
    public virtual async Task<(BaseGameSession session, GuessResult result)> SubmitGuessAsync(
        Guid sessionId, 
        Guid playerId, 
        int targetIndex, 
        string? titleGuess, 
        string? artistGuess)
    {   
        // get the session including its players from the database
        var session = await GetSessionWithSortedPlayersAsync(sessionId);

        if (session == null || session.Status != SessionStatus.InProgress)
            throw new Exception("Game session not found or not active.");

        var currentPlayer = session.Players[session.CurrentPlayerIndex];
        if (currentPlayer.Id != playerId)
            throw new Exception("It's not your turn!");

        // get the actual song to compare guesses
        var actualSong = session.CurrentActiveSong!;
        var result = new GuessResult
        {
            PlayerId = playerId,
            CorrectTitle = actualSong.Title,
            CorrectArtist = actualSong.Artist,
            CorrectYear = actualSong.ReleaseYear
        };

        // check if the user's placement of the card is correct
        result.PlacementCorrect = VerifyPlacement(currentPlayer.Timeline, actualSong.ReleaseYear, targetIndex);

        // if its correct
        if (result.PlacementCorrect)
        {
            // add the card to the player timeline at the specified index
            currentPlayer.Timeline.Insert(targetIndex, new TimelineCard
            {
                SongId = actualSong.Id,
                Title = actualSong.Title,
                Artist = actualSong.Artist,
                ReleaseYear = actualSong.ReleaseYear
            });
            currentPlayer.Timeline = currentPlayer.Timeline.ToList(); // update for EF Core

            // check for bonus token (correct title and artist)
            bool titleMatch = string.Equals(titleGuess?.Trim(), actualSong.Title, StringComparison.OrdinalIgnoreCase);
            bool artistMatch = string.Equals(artistGuess?.Trim(), actualSong.Artist, StringComparison.OrdinalIgnoreCase);

            if (titleMatch && artistMatch)
            {
                currentPlayer.Tokens++;
                result.BonusEarned = true;
            }
        }

        // manage turns and move to the next song
        session.CurrentPlayerIndex = (session.CurrentPlayerIndex + 1) % session.Players.Count;
        await PrepareNextActiveSong(session);

        await _context.SaveChangesAsync();
        return (session, result);
    }

    // method to prepare the next active song for the session
    protected async Task PrepareNextActiveSong(BaseGameSession session)
    {   
        // check for victory condition before drawing a new song
        var winner = session.Players.FirstOrDefault(p => p.Timeline.Count >= session.Config!.WinningScore);
        if (winner != null)
        {
            await HandleVictoryAsync(session, winner);
            return; // stop here - do not draw a new song
        }
        var songService = _songServiceFactory.GetService(session.Config.PlaylistUrl);
        
        // draw one new song that hasn't appeared yet
        var nextSongs = await songService.GetRandomSongsAsync(
            1, 
            session.Config.Languages.ToArray(), 
            session.PlayedSongIds.ToArray(), 
            null, null); 
        
        // if no more songs are available, end the game
        if (!nextSongs.Any())
        {
            session.GameOverReason = "Out of songs";
            
            // get the winner based on the highest timeline count
            var topPlayer = session.Players
                .OrderByDescending(p => p.Timeline.Count)
                .ThenByDescending(p => p.Tokens)
                .First();

            await HandleVictoryAsync(session, topPlayer);
            return;
        }

        var nextSong = nextSongs.First();
        session.CurrentActiveSong = nextSong;
        session.PlayedSongIds.Add(nextSong.Id.ToString());
        session.PlayedSongIds = session.PlayedSongIds.ToList();
        
    } 

    protected virtual async Task HandleVictoryAsync(BaseGameSession session, Player winner)
    {
        session.Status = SessionStatus.Finished;
        await _context.SaveChangesAsync();

    }
}