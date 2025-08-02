using RealTimeChatServer.Websockets.Interfaces;
using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Websockets.Commands;

public class BroadcastMessageCommand(IMessageCommandProcessor messageCommandProcessor) : IWebSocketCommand
{
	public string Name => "broadcastMessage";

	public async Task ExecuteAsync(WsRequestDto request, string socketId)
	{
		if (request.Message == null)
			throw new ArgumentNullException(nameof(request.Message));

		await messageCommandProcessor.BroadcastMessage(request.Message.ToModel(), socketId);
	}
}