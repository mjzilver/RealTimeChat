using RealTimeChatServer.Websockets.Interfaces;
using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Websockets.Commands;

public class DeleteChannelCommand(IChannelCommandProcessor channelCommandProcessor) : IWebSocketCommand
{
	public string Name => "deleteChannel";

	public async Task ExecuteAsync(WsRequestDto request, string socketId)
	{
		if (request.Channel?.Id is not int deleteId)
			throw new ArgumentNullException("Channel ID is required for deletion");

		await channelCommandProcessor.DeleteChannel(deleteId, socketId);
	}
}
    