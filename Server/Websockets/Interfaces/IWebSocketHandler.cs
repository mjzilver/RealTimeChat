using System.Net.WebSockets;

namespace B4mServer.Websockets.Interfaces;

public interface IWebSocketHandler
{
    public Task Handle();
}