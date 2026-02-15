using System.Text.Json;

using RealTimeChatServer.Websockets.Interfaces;
using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Websockets.Commands;

public class DeleteChannelCommand(IChannelCommandProcessor channelCommandProcessor, JsonSerializerOptions options) : IWebSocketCommand
{
	public string Name => "deleteChannel";

	public async Task ExecuteAsync(WsRequestDto request, string socketId)
	{
		if (request.Payload == null)
			throw new ArgumentNullException(nameof(request));

		var payload = JsonSerializer.Deserialize<DeleteChannelPayload>(request.Payload.Value.GetRawText(), options) ?? throw new ArgumentException("Invalid delete channel payload");
		await channelCommandProcessor.DeleteChannel(payload.Id, socketId);
	}
}
