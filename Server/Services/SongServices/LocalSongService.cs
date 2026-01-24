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

    public async Task<Song?> GetRandomSongAsync(string[]? languages, string[]? excludedIds)
    {
        IQueryable<Song> query = _context.Songs;

        if (languages?.Length > 0)
        {   
            // exclude songs that are not in the specified languages
            query = query.Where(s => languages.Contains(s.Language));
        }

        if (excludedIds?.Length > 0)
        {
            // exclude songs that already used in the current game
            query = query.Where(s => !excludedIds.Contains(s.Id.ToString()));
        }

        return await query.OrderBy(s => Guid.NewGuid()).FirstOrDefaultAsync();
    }
}