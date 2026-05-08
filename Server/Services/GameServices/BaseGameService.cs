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

    // Verify if the placing in the timeline is correct
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

        // Fetch anchor cards for everyone + the first active song in one call
        int totalSongsNeeded = session.Players.Count + 1;

        // Fetch "anchor" cards for everyone + one for active song.
        // we fetch for each player one random card 
        // there is an option to include 2 more arguments for minYear nad maxYear for filtering by year range (currently not used)
        var allSongs = await songService.GetRandomSongsAsync(
            totalSongsNeeded, 
            session.Config.Languages.ToArray(), 
            null, null, null);

        // each player gets one anchor card added to their timeline
        for (int i = 0; i < session.Players.Count; i++)
        {
            var song = allSongs[i];
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

        // The last song in the batch becomes the first active song
        var activeSong = allSongs.Last();
        session.CurrentActiveSong = activeSong;
        session.PlayedSongIds.Add(session.CurrentActiveSong.Id.ToString());

        // for the erf to recognize changes in the played song ids list
        session.PlayedSongIds = session.PlayedSongIds.ToList(); 
        
        // updating status and saving
        session.Status = SessionStatus.InProgress;
        await _context.SaveChangesAsync();

        return session;
    } 

    
    // method that processes a player's guess submission
    public virtual async Task<(BaseGameSession session, GuessResult result, string? winnerName)> SubmitGuessAsync(
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

        string? winnerName = null;
        // check if the player has won after this guess
        bool hasWon = await CheckForAWinner(session); 
        if (hasWon)
        {
            winnerName = currentPlayer.Name;
        }
        else
        {
            // Prepare the next song and capture the winnerName if it triggers "Out of songs" case
            winnerName = await PrepareNextActiveSong(session);
            // If there's still no winner, we move to the next player in the turn order.
            if (winnerName == null)
            {
                session.CurrentPlayerIndex = (session.CurrentPlayerIndex + 1) % session.Players.Count;
            }
        }
        
        await _context.SaveChangesAsync();
        return (session, result, winnerName);
    }

    // Method to prepare the next active song for the session
    protected async Task<string?> PrepareNextActiveSong(BaseGameSession session)
    {   
        var songService = _songServiceFactory.GetService(session.Config.PlaylistUrl);
        // Draw one new song that hasn't appeared yet
        var nextSongs = await songService.GetRandomSongsAsync(
            1, 
            session.Config.Languages.ToArray(), 
            session.PlayedSongIds.ToArray(), 
            null, null); 
        
        // If no more songs are available, we must determine the winner by points.
        if (!nextSongs.Any())
        {
            return await HandleOutOfSongsAsync(session);
        }

        // Set the new song as the current active one
        var nextSong = nextSongs.First();
        session.CurrentActiveSong = nextSong;
        
        // Track played songs to avoid repeats
        session.PlayedSongIds.Add(nextSong.Id.ToString());
        session.PlayedSongIds = session.PlayedSongIds.ToList(); // Update for EF Core tracking

        return null; // Game continues
    } 

    // function to check if the last player won after last guess.
    protected async Task<bool> CheckForAWinner(BaseGameSession session)
    {
        // if the current player has reached the winning score, we finish the game.
        var currentPlayer = session.Players[session.CurrentPlayerIndex];
        if (currentPlayer.Timeline.Count >= session.Config!.WinningScore)
        {
            await HandleVictoryAsync(session, currentPlayer);
            return true;
        }
        
        return false;
    }

    protected virtual async Task HandleVictoryAsync(BaseGameSession session, Player winner)
    {
        session.Status = SessionStatus.Finished;
        await _context.SaveChangesAsync();

    } 

    // Handles the end of the game when no more songs are available in the pool
    protected async Task<string> HandleOutOfSongsAsync(BaseGameSession session)
    {
        session.GameOverReason = "Out of songs";
        
        // We iterate here to compare all players and find the global winner
        var topPlayer = session.Players
            .OrderByDescending(p => p.Timeline.Count)
            .ThenByDescending(p => p.Tokens)
            .First();

        // Trigger the victory logic for the top player and return the winner's name.
        await HandleVictoryAsync(session, topPlayer);
        return topPlayer.Name;
    }
}