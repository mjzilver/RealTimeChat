using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Websockets.Interfaces;

public interface IWebSocketCommand
{
	string Name { get; }
	Task ExecuteAsync(WsRequestDto context, string socketId);
}