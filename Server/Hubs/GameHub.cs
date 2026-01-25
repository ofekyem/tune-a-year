using Microsoft.AspNetCore.SignalR;

namespace Server.Hubs;

public class GameHub : Hub
{
    // join a room based on the provided room code
    public async Task JoinRoom(string roomCode)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode.ToUpper());
    }
}