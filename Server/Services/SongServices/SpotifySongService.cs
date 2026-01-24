using Server.Models.Music;

namespace Server.Services.SongServices;
public class SpotifySongService : ISongService
{
    public Task<List<Song>> GetRandomSongsAsync(int count, string[]? languages, string[]? excludedIds, int? minYear = null, int? maxYear = null)
    {
        // implement when Spotify API access is available
        throw new NotImplementedException("spotify service not implemented yet");
    }
}