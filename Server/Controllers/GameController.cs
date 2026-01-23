using Microsoft.AspNetCore.Mvc;
using Server.Models.Game;
using Server.Models.Game.Sessions;
using Server.Models.Game.Players;
using Server.Services.GameServices;
using Server.Services.Factories;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly GameServiceFactory _gameFactory;

    // Injecting the game service factory 
    public GameController(GameServiceFactory gameFactory)
    {
        _gameFactory = gameFactory;
    }

    /// Create a new game session (lobby)
    [HttpPost("create")]
    public async Task<ActionResult<BaseGameSession>> CreateGame([FromBody] MatchConfiguration config)
    {
        try
        {   
            var gameService = _gameFactory.GetService(config.Mode); 

            var session = await gameService.CreateGameAsync(config);
            // Return 201 Created with the new session info (including the RoomCode)
            return Ok(session);
        }
        catch (ArgumentNullException ex)
        {   
            Console.WriteLine("argument null exception caught in CreateGame:");
            Console.WriteLine(ex.ToString());
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex) // הוספנו את המשתנה ex
        {
            // מדפיס את השגיאה המלאה, כולל ה-Stack Trace, לטרמינל של ה-dotnet run
            Console.WriteLine("========= ERROR IN CREATE GAME =========");
            Console.WriteLine(ex.ToString());
            Console.WriteLine("========================================");
            
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    } 

    /// Join an existing game session by RoomCode for online service
    [HttpPost("join")]
    public async Task<ActionResult<Player>> JoinGame([FromQuery] string roomCode, [FromQuery] string playerName)
    {
        try
        {
            // here we specifically use the OnlineGameService to join by code
            var onlineService = (OnlineGameService)_gameFactory.GetService(GameMode.Online);
            var player = await onlineService.JoinByCodeAsync(roomCode, playerName);
            return Ok(player);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

}