using System.Net.WebSockets;

namespace RealTimeChatServer.Websockets.Interfaces;

public interface IWebSocketSender
{
	Task SendAsync(WebSocket socket, string message);
	Task SendErrorAsync(string socketId, string errorMessage);
}
