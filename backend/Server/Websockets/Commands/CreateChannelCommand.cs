using RealTimeChatServer.Websockets.Interfaces;
using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Websockets.Commands;

public class CreateChannelCommand(IChannelCommandProcessor channelCommandProcessor) : IWebSocketCommand
{
	public string Name => "createChannel";

	public async Task ExecuteAsync(WsRequestDto request, string socketId)
	{
		if (request.Channel == null)
			throw new ArgumentNullException(nameof(request.Channel));

		await channelCommandProcessor.CreateChannel(request.Channel.ToModel(), socketId);
	}
}