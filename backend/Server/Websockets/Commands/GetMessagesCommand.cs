using System.Text.Json;

using RealTimeChatServer.Websockets.Interfaces;
using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Websockets.Commands;

public class GetMessagesCommand(IMessageCommandProcessor messageCommandProcessor, JsonSerializerOptions options) : IWebSocketCommand
{
	public string Name => "getMessages";

	public async Task ExecuteAsync(WsRequestDto request, string socketId)
	{
		if (request.Payload == null)
			throw new ArgumentNullException(nameof(request));

		var payload = JsonSerializer.Deserialize<GetMessagesPayload>(request.Payload.Value.GetRawText(), options) ?? throw new ArgumentException("Invalid getMessages payload");
		await messageCommandProcessor.GetMessages(payload.ChannelId, socketId);
	}
}
