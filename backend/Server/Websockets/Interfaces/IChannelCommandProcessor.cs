using RealTimeChatServer.Models;

namespace RealTimeChatServer.Websockets.Interfaces;

public interface IChannelCommandProcessor
{
	Task GetChannels(string socketId);
	Task CreateChannel(Channel channel, string socketId);
	Task DeleteChannel(int channelId, string socketId);
	Task UpdateChannel(Channel channel, string socketId);
	Task JoinChannel(int channelId, int userId);
	Task LeaveChannel(int channelId, int userId);
}