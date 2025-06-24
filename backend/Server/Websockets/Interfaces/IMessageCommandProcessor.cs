using RealTimeChatServer.Models;

namespace RealTimeChatServer.Websockets.Interfaces;

public interface IMessageCommandProcessor
{
	Task BroadcastMessage(Message message, string socketId);
	Task GetMessages(int channelId, string socketId);
}
