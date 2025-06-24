using B4mServer.Models;

namespace B4mServer.Websockets.Interfaces;

public interface IMessageCommandProcessor
{
    Task BroadcastMessage(Message message, string socketId);
    Task GetMessages(int channelId, string socketId);
}
