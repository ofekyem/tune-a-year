using Microsoft.AspNetCore.Mvc;
using Server.Models.Game;
using Server.Models.Game.Sessions;
using Server.Services.GameServices;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;

    // Injecting the service we created in the previous step
    public GameController(IGameService gameService)
    {
        _gameService = gameService;
    }

    /// Create a new game session (lobby)
    [HttpPost("create")]
    public async Task<ActionResult<BaseGameSession>> CreateGame([FromBody] MatchConfiguration config)
    {
        try
        {
            var session = await _gameService.CreateGameAsync(config);
            // Return 201 Created with the new session info (including the RoomCode)
            return Ok(session);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while creating the game session.");
        }
    }

    // will add later: JoinGame, GetStatus, etc.
}