using RealTimeChatServer.Websockets.Interfaces;
using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Websockets.Commands;

public class UpdateChannelCommand(IChannelCommandProcessor channelCommandProcessor) : IWebSocketCommand
{
	public string Name => "updateChannel";

	public async Task ExecuteAsync(WsRequestDto request, string socketId)
	{
		if (request.Channel == null)
			throw new ArgumentNullException(nameof(request.Channel));

		await channelCommandProcessor.UpdateChannel(request.Channel.ToModel(), socketId);
	}
}