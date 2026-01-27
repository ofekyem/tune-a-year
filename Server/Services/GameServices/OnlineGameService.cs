using Server.Data;
using Server.Models.Game;
using Server.Models.Game.Sessions;
using Server.Models.Game.Players; 
using Server.Models.Game.Timeline;
using Server.Services.SongServices;
using Server.Services.Factories;
using Microsoft.EntityFrameworkCore; 
using Microsoft.AspNetCore.SignalR;
using Server.Hubs;

namespace Server.Services.GameServices; 

public class OnlineGameService : BaseGameService
{   
    private readonly IHubContext<GameHub> _hubContext;
    public OnlineGameService(AppDbContext context, SongServiceFactory songServiceFactory, IHubContext<GameHub> hubContext) : base(context, songServiceFactory) 
    { 
        _hubContext = hubContext;
    }

    public override async Task<BaseGameSession> CreateGameAsync(MatchConfiguration config)
    {   
        
        if (string.IsNullOrEmpty(config.CreatorName))
        {
            throw new Exception("CreatorName is required for online games.");
        }
        var session = InitializeSession(config);
        // logic of unique RoomCode
        string code;
        bool isCodeTaken;
        do
        {
            code = GenerateRoomCode();
            isCodeTaken = await _context.GameSessions
                .AnyAsync(s => s.RoomCode == code && s.Status != SessionStatus.Finished);
        } while (isCodeTaken);

        session.RoomCode = code;

        // create the host player
        var hostPlayer = new OnlinePlayer
        {
            Name = config.CreatorName,
            Tokens = 2,
            Timeline = new List<TimelineCard>(),
            BaseGameSessionId = session.Id, 
            IsHost = true, 
            IsConnected = true,
            JoinOrder = 0
        };

        _context.GameSessions.Add(session);
        _context.Players.Add(hostPlayer);

        await _context.SaveChangesAsync();
        return session;
    } 

    // player joins an existing game by room code
    public async Task<BaseGameSession> JoinByCodeAsync(string roomCode, string playerName)
    {
        // get game session by code from the database
        var sessionData = await _context.GameSessions
            .AsNoTracking()
            .Where(s => s.RoomCode == roomCode.ToUpper() && s.Status == SessionStatus.Lobby)
            .Select(s => new 
            { 
                s.Id, 
                s.Config.MaxPlayers, 
                CurrentPlayerCount = s.Players.Count 
            })
            .FirstOrDefaultAsync();

        // if not found then the room code is invalid or game already started
        if (sessionData == null) 
            throw new Exception("the room code is invalid or the game has already started.");

        // check if there is space
        if (sessionData.CurrentPlayerCount >= sessionData.MaxPlayers)
            throw new Exception("the room is full.");
        // create the new player for the session
        var player = new OnlinePlayer
        {
            Name = playerName,
            Tokens = 2, // 2 starting tokens
            Timeline = new List<TimelineCard>(), // empty timeline to be filled in StartGame
            BaseGameSessionId = sessionData.Id, 
            IsHost = false, 
            IsConnected = true,
            JoinOrder = sessionData.CurrentPlayerCount
        };

        _context.Players.Add(player);
        await _context.SaveChangesAsync(); 

        var fullSession = await _context.GameSessions
            .Include(s => s.Players)
            .FirstOrDefaultAsync(s => s.Id == sessionData.Id);
        
        if (fullSession == null) throw new Exception("Session synchronization failed.");

        // notify all clients in the room that a new player has joined
        await _hubContext.Clients.Group(roomCode.ToUpper())
            .SendAsync("PlayerJoined", player);
        
        return fullSession!;
    } 


    public override async Task<BaseGameSession> StartGameAsync(Guid sessionId)
    {
        var session = await base.StartGameAsync(sessionId);

        
        // notify all clients in the room that the game has started and send the updated state (including the first song)
        await _hubContext.Clients.Group(session.RoomCode!)
            .SendAsync("GameStarted", session);

        return session;
    }

    // private function to generate the room code
    private string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; 
        return new string(Enumerable.Repeat(chars, 4)
            .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
    } 

    public override async Task<(BaseGameSession session, GuessResult result)> SubmitGuessAsync(
        Guid sessionId, 
        Guid playerId, 
        int targetIndex, 
        string? titleGuess, 
        string? artistGuess)
    {
        // call the base method to process the guess
        var (session, result) = await base.SubmitGuessAsync(sessionId, playerId, targetIndex, titleGuess, artistGuess);


        // broadcast the results to all clients in the room
        await _hubContext.Clients.Group(session.RoomCode!)
            .SendAsync("GuessResultReceived", result);

        // broadcast the updated game state to all clients in the room
        await _hubContext.Clients.Group(session.RoomCode!)
            .SendAsync("GameUpdated", session);

        return (session, result);
    } 

    protected override async Task HandleVictoryAsync(BaseGameSession session, Player winner)
    {
        // call the base method to handle victory logic
        await base.HandleVictoryAsync(session, winner);

        // broadcast the game over event to all clients in the room
        await _hubContext.Clients.Group(session.RoomCode!)
            .SendAsync("GameOver", new 
            { 
                winnerName = winner.Name, 
                finalSession = session 
            });
    }

}