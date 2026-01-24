using Server.Models.Music;

namespace Server.Services.SongServices;

// interface for the options of song service
public interface ISongService
{
    // method that gets count of random songs in the given languages excluding the given songs and in range of minYear and maxYear
    Task<List<Song>> GetRandomSongsAsync(int count, string[]? languages, string[]? excludedIds, int? minYear = null, int? maxYear = null);
}