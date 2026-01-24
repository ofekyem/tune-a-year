using Server.Models.Game;
using Server.Services.GameServices;

namespace Server.Services.Factories;

public class GameServiceFactory
{
    private readonly IServiceProvider _serviceProvider;

    public GameServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IGameService GetService(GameMode mode)
    {
        return mode switch
        {
            // get the appropriate game service based on the game mode
            GameMode.SingleDevice => _serviceProvider.GetRequiredService<LocalGameService>(),
            GameMode.Online => _serviceProvider.GetRequiredService<OnlineGameService>(),
            _ => throw new ArgumentException("Invalid game mode selected.")
        };
    }
}