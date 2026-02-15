using System.Text.Json;

using RealTimeChatServer.Websockets.Interfaces;
using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Websockets.Commands;

public class BroadcastMessageCommand(IMessageCommandProcessor messageCommandProcessor, JsonSerializerOptions options) : IWebSocketCommand
{
	public string Name => "broadcastMessage";

	public async Task ExecuteAsync(WsRequestDto request, string socketId)
	{
		if (request.Payload == null)
			throw new ArgumentNullException(nameof(request));

		var payload = JsonSerializer.Deserialize<BroadcastMessagePayload>(request.Payload.Value.GetRawText(), options) ?? throw new ArgumentException("Invalid message payload");
		var message = new Models.Message
		{
			UserId = payload.UserId,
			ChannelId = payload.ChannelId,
			Text = payload.Text
		};

		await messageCommandProcessor.BroadcastMessage(message, socketId);
	}
}