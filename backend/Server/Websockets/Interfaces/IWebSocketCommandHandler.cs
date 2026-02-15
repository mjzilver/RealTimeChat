using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Websockets.Interfaces;

public interface IWebSocketCommandHandler
{
	string CommandName { get; }
	Task HandleAsync(WsRequestDto request, string socketId);
}
