using Server.Services;

namespace Server.Services.Factories;

public class SongServiceFactory
{
    private readonly IServiceProvider _serviceProvider;

    public SongServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ISongService GetService(string? playlistId)
    {
        // if a playlistId is provided, use the Spotify playlist service
        if (!string.IsNullOrEmpty(playlistId))
        {
            return _serviceProvider.GetRequiredService<SpotifySongService>();
        }

        // the default is the local database service
        return _serviceProvider.GetRequiredService<LocalSongService>();
    }
}