using Server.Models;

namespace Server.Services;
public class SpotifySongService : ISongService
{
    public async Task<Song?> GetRandomSongAsync(string[]? languages, string[]? excludedIds)
    {
        // implement when Spotify API access is available
        throw new NotImplementedException("spotify service not implemented yet");
    }
}