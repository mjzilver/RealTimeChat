namespace RealTimeChatServer.Websockets.Interfaces;

public interface IWebSocketCommandProcessor
{
	Task ProcessCommandAsync(WebSocketCommand command, string socketId);
	void UserDisconnected(string socketId);
}