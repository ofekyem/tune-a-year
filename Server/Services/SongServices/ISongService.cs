using Server.Models.Music;

namespace Server.Services.SongServices;

// interface for the options of song service
public interface ISongService
{
    // each service must contain method that gets a random song
    Task<Song?> GetRandomSongAsync(string[]? languages, string[]? excludedIds);
}