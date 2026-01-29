using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Models.Game;
using Server.Data;
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
    private readonly AppDbContext _context;

    // Injecting the game service factory 
    public GameController(GameServiceFactory gameFactory, AppDbContext context)
    {
        _gameFactory = gameFactory;
        _context = context;
    }

    // Create a new game session (lobby)
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
        catch (Exception ex) 
        {
            // debug error logging
            Console.WriteLine("========= ERROR IN CREATE GAME =========");
            Console.WriteLine(ex.ToString());
            Console.WriteLine("========================================");
            
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    } 

    // Join an existing game session by RoomCode for online service
    [HttpPost("join")]
    public async Task<ActionResult<BaseGameSession>> JoinGame([FromQuery] string roomCode, [FromQuery] string playerName)
    {
        try
        {
            // here we specifically use the OnlineGameService to join by code
            var onlineService = (OnlineGameService)_gameFactory.GetService(GameMode.Online);
            var (session, playerId) = await onlineService.JoinByCodeAsync(roomCode, playerName);
            return Ok(new { session, playerId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    } 

    // start the online game after host request to
    [HttpPost("{id}/start")]
    public async Task<ActionResult> StartOnlineGame(Guid id)
    {
        // !!!! add later authorization to ensure only host can start the game !!!!
        var onlineService = (OnlineGameService)_gameFactory.GetService(GameMode.Online);
        var session = await onlineService.StartGameAsync(id);
        return Ok(session);
    } 

    // method after player submits a guess
    [HttpPost("{id}/guess")]
    public async Task<ActionResult> SubmitGuess(
        Guid id, 
        [FromQuery] Guid playerId, 
        [FromQuery] int targetIndex, 
        [FromQuery] string? titleGuess, 
        [FromQuery] string? artistGuess)
    {
        try
        {
            // get the session to determine which service to use
            var session = await _context.GameSessions.FindAsync(id);
            if (session == null) return NotFound(new { message = "Session not found" });

            var gameService = _gameFactory.GetService(session.Config.Mode);
            var (updatedSession, result) = await gameService.SubmitGuessAsync(id, playerId, targetIndex, titleGuess, artistGuess);
            
            // return the result (success/failure message) and the updated session state
            return Ok(new { session = updatedSession, result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // The endpoint to clean up the game after it ends
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteGame(Guid id)
    {
        try
        {
            var session = await _context.GameSessions
                .Include(s => s.Players)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session == null) return NotFound();

            // Deleting the session will automatically delete the players thanks to Cascade Delete in the DB
            _context.GameSessions.Remove(session);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Game session cleaned up successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    } 

    // Get the current state of a session
    [HttpGet("{id}")]
    public async Task<ActionResult<BaseGameSession>> GetSession(Guid id)
    {
        var session = await _context.GameSessions
            .Include(s => s.Players)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (session == null) return NotFound();

        session.Players = session.Players
            .OrderBy(p => p.JoinOrder)
            .ToList();

        return Ok(session);
    }
}
