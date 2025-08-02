using RealTimeChatServer.Websockets.Interfaces;
using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Websockets.Commands;

public class JoinChannelCommand(IChannelCommandProcessor channelCommandProcessor) : IWebSocketCommand
{
	public string Name => "joinChannel";

	public async Task ExecuteAsync(WsRequestDto request, string socketId)
	{
		if (request.Channel?.Id is not int joinChannelId || request.User?.Id is not int joinUserId)
			throw new ArgumentException("Channel ID and User ID are required for joining");

		await channelCommandProcessor.JoinChannel(joinChannelId, joinUserId);
	}
}