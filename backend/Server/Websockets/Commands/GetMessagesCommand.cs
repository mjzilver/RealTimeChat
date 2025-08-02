using RealTimeChatServer.Websockets.Interfaces;
using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Websockets.Commands;

public class GetMessagesCommand(IMessageCommandProcessor messageCommandProcessor) : IWebSocketCommand
{
	public string Name => "getMessages";

	public async Task ExecuteAsync(WsRequestDto request, string socketId)
	{
		if (request.Channel?.Id is not int channelId)
			throw new ArgumentNullException("Channel ID is required");

		await messageCommandProcessor.GetMessages(channelId, socketId);
	}
}
