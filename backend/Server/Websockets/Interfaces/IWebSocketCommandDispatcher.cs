using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Websockets.Interfaces;

public interface IWebSocketCommandDispatcher
{
	Task ProcessCommandAsync(WsRequestDto request, string socketId);
	Task HandleDisconnectionAsync(string socketId);
}