using Server.Data;
using Server.Models.Game;
using Server.Models.Game.Sessions;
using Server.Models.Game.Players; 
using Server.Models.Game.Timeline;
using Server.Services.SongServices;
using Server.Services.Factories;
using Microsoft.EntityFrameworkCore; 

namespace Server.Services.GameServices; 

public class OnlineGameService : BaseGameService
{
    public OnlineGameService(AppDbContext context, SongServiceFactory songServiceFactory) : base(context, songServiceFactory) { }

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
            IsConnected = true
        };

        _context.GameSessions.Add(session);
        _context.Players.Add(hostPlayer);

        await _context.SaveChangesAsync();
        return session;
    } 

    // player joins an existing game by room code
    public async Task<Player> JoinByCodeAsync(string roomCode, string playerName)
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
            IsConnected = true
        };

        _context.Players.Add(player);
        await _context.SaveChangesAsync();
        
        return player;
    }

    // private function to generate the room code
    private string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; 
        return new string(Enumerable.Repeat(chars, 4)
            .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
    }

}