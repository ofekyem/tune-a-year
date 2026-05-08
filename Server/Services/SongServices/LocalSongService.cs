using Server.Data; 
using Server.Models.Music;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Server.Services.SongServices;

public class LocalSongService : ISongService
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    

    public LocalSongService(AppDbContext context, HttpClient httpClient)
    {
        _context = context;
        _httpClient = httpClient;
    } 

    // Ensure that the song has a fresh preview URL from Deezer
    public async Task EnsureFreshPreviewAsync(Song song)
    {
        if (string.IsNullOrEmpty(song.SpotifyId)) return;

        try {
            // get the preview URL from Deezer API
            var response = await _httpClient.GetAsync($"https://api.deezer.com/track/{song.SpotifyId}");
            if (response.IsSuccessStatusCode) {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("preview", out var previewProp)) {
                    song.PreviewUrl = previewProp.GetString() ?? "";
                }
            }
        } catch (Exception ex) {
            Console.WriteLine($"Call for deezer in EnsureFreshPreviewAsync failed: {ex.Message}");
        }
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

        // get random songs from the filtered query
        var songs = await query.OrderBy(s => EF.Functions.Random()).Take(count).ToListAsync();
        
        // ensure each song has a fresh preview URL from Deezer
        var tasks = songs.Select(s => EnsureFreshPreviewAsync(s));
        await Task.WhenAll(tasks); 

        return songs;
    }
}