using Server.Data; 
using Server.Models.Music;
using Microsoft.EntityFrameworkCore;

namespace Server.Services.SongServices;

public class LocalSongService : ISongService
{
    private readonly AppDbContext _context;

    public LocalSongService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Song>> GetRandomSongsAsync(int count, string[]? languages, string[]? excludedIds, int? minYear = null, int? maxYear = null)
    {   
        // build the query that will be sent to the database
        IQueryable<Song> query = _context.Songs;

        // if year range is specified, filter by it
        if (minYear.HasValue) query = query.Where(s => s.ReleaseYear >= minYear.Value);
        if (maxYear.HasValue) query = query.Where(s => s.ReleaseYear <= maxYear.Value);

        // exclude songs that are not in the specified languages
        if (languages?.Length > 0)
        {   
            query = query.Where(s => languages.Contains(s.Language));
        }

        // exclude songs that already used in the current game
        if (excludedIds?.Length > 0)
        {
            query = query.Where(s => !excludedIds.Contains(s.Id.ToString()));
        }

        // return a "count" number of random songs from the filtered query
        return await query
            .OrderBy(s => EF.Functions.Random())
            .Take(count)
            .ToListAsync();
    }
}