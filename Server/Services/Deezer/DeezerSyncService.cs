using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models.Game; 

namespace Server.Services.Deezer
{
    public class DeezerSyncService
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;

        public DeezerSyncService(AppDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        // this method syncs Deezer IDs for songs missing them
        public async Task SyncDeezerIdsAsync()
        {
            // get songs missing Deezer IDs (SpotifyId field used for Deezer ID here)
            var songsToUpdate = await _context.Songs
                .Where(s => s.SpotifyId.Length < 5 || s.PreviewUrl.StartsWith("url_"))
                .ToListAsync();

            Console.WriteLine($"[DeezerSync] Starting ID sync for {songsToUpdate.Count} songs.");

            foreach (var song in songsToUpdate)
            {
                try
                {
                    // search Deezer API by song title and artist
                    string cleanTitle = song.Title.Replace("...", "").Trim();
                    string cleanArtist = song.Artist.Trim();
                    var searchString = $"{cleanTitle} {cleanArtist}";
                    
                    var url = $"https://api.deezer.com/search?q={Uri.EscapeDataString(searchString)}";
                    
                    var response = await _httpClient.GetAsync(url);
                    if (!response.IsSuccessStatusCode) continue;

                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("data", out var dataArray) && dataArray.GetArrayLength() > 0)
                    {
                        // Extracting the unique Deezer ID (represented as a number in JSON)
                        var deezerId = dataArray[0].GetProperty("id").GetInt64().ToString();
                        
                        if (!string.IsNullOrEmpty(deezerId))
                        {
                            // Saving the permanent ID in the SpotifyId field
                            song.SpotifyId = deezerId;
                            
                            // Marking the song as synced so it won't be processed again
                            song.PreviewUrl = "ID_SYNCED"; 

                            Console.WriteLine($"[DeezerSync] ID SAVED: {song.Title} -> {deezerId}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[DeezerSync] NOT FOUND ON DEEZER: {song.Title}");
                    }

                    // Delay to prevent hitting Rate Limit
                    await Task.Delay(200); 
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DeezerSync] Error processing {song.Title}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
            Console.WriteLine("[DeezerSync] ID synchronization complete.");
        }
    }
}