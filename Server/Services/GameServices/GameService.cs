// using Server.Data;
// using Server.Models.Game;
// using Server.Models.Game.Sessions;
// using Microsoft.EntityFrameworkCore;

// namespace Server.Services.GameServices;

// public class GameService : IGameService
// {
//     private readonly AppDbContext _context;

//     public GameService(AppDbContext context)
//     {
//         _context = context;
//     }

//     public async Task<BaseGameSession> CreateGameAsync(MatchConfiguration config)
//     {
//         // check for null config
//         if (config == null) throw new ArgumentNullException(nameof(config));

//         // 2. create the appropriate session type
//         BaseGameSession session = config.Source == MusicSource.LocalDatabase 
//             ? new LocalDatabaseSession() 
//             : new ExternalPlaylistSession();

//         session.Config = config;
//         session.Status = SessionStatus.Lobby;
//         session.CreatedAt = DateTime.UtcNow;

//         // unique RoomCode logic
//         string code;
//         bool isCodeTaken;
//         do
//         {
//             code = GenerateRoomCode();
//             // check if there is an active game with the same code
//             isCodeTaken = await _context.GameSessions
//                 .AnyAsync(s => s.RoomCode == code && s.Status != SessionStatus.Finished);
//         } while (isCodeTaken);

//         session.RoomCode = code;

//         // save to the database
//         _context.GameSessions.Add(session);
//         await _context.SaveChangesAsync();

//         return session;
//     }

//     public string GenerateRoomCode()
//     {
//         // Using Shared Random (more efficient and safer in .NET 6 and above)
//         // Removed characters that look similar (like O and 0, or I and 1) to prevent player mistakes
//         const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; 
        
//         return new string(Enumerable.Repeat(chars, 4)
//             .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
//     }
// }