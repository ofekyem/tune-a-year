using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models.Music;
using Server.Services.SongServices; 
using Server.Services.Factories;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]

public class SongsController : ControllerBase
{
    private readonly AppDbContext _context; 
    private readonly SongServiceFactory _serviceFactory;
    public SongsController(AppDbContext context, SongServiceFactory serviceFactory)
    {
        _context = context;
        _serviceFactory = serviceFactory;
    } 

    // Get all songs from the database
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Song>>> GetSongs()
    {
        return await _context.Songs.ToListAsync();
    }  

    // Get a specific song by ID from the database
    [HttpGet("{id}")]
    public async Task<ActionResult<Song>> GetSong(Guid id)
    {
        var song = await _context.Songs.FindAsync(id);
        if (song == null)
        {
            return NotFound(); 
        }
        return song;
    }

    // Add a new song to the database
    [HttpPost]
    public async Task<ActionResult<Song>> PostSong(Song song)
    {
        _context.Songs.Add(song);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetSong), new { id = song.Id }, song);
    } 

    [HttpGet("random")]
    public async Task<ActionResult<Song>> GetRandomSong(
        [FromQuery] int count = 1, // defualt to 1 song
        [FromQuery] string[]? languages = null, 
        [FromQuery] string[]? excludedIds = null, 
        [FromQuery] string? playlistId = null,
        [FromQuery] int? minYear = null,
        [FromQuery] int? maxYear = null)
    {
        // get the appropriate song service based on the presence of playlistId
        var selectedService = _serviceFactory.GetService(playlistId);

        // fetch random songs using the selected service
        var songs = await selectedService.GetRandomSongsAsync(count, languages, excludedIds, minYear, maxYear);

        if (songs == null || songs.Count == 0) return NotFound("No song found matching the criteria.");
        
        return Ok(songs);
    }

    

}