using B4mServer.Models;

namespace B4mServer.Websockets.Interfaces;

public interface IWebSocketCommandProcessor
{
    Task ProcessCommandAsync(WebSocketCommand command, string socketId);
    void UserDisconnected(string socketId);
}