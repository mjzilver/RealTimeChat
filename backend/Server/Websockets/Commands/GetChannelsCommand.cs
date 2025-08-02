using RealTimeChatServer.Websockets.Interfaces;
using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Websockets.Commands;

public class GetChannelsCommand(IChannelCommandProcessor channelCommandProcessor) : IWebSocketCommand
{
	public string Name => "getChannels";

	public async Task ExecuteAsync(WsRequestDto request, string socketId)
	{
		await channelCommandProcessor.GetChannels(socketId);
	}
}