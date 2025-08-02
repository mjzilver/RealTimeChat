using RealTimeChatServer.Websockets.Interfaces;
using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Websockets.Commands;

public class LeaveChannelCommand(IChannelCommandProcessor channelCommandProcessor) : IWebSocketCommand
{
	public string Name => "leaveChannel";

	public async Task ExecuteAsync(WsRequestDto request, string socketId)
	{
		if (request.Channel?.Id is not int leaveChannelId || request.User?.Id is not int leaveUserId)
			throw new ArgumentException("Channel ID and User ID are required for leaving");

		await channelCommandProcessor.LeaveChannel(leaveChannelId, leaveUserId);
	}
}