using System.Text.Json;

using RealTimeChatServer.Websockets.Interfaces;
using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Websockets.Commands;

public class JoinChannelCommand(IChannelCommandProcessor channelCommandProcessor, JsonSerializerOptions options) : IWebSocketCommand
{
	public string Name => "joinChannel";

	public async Task ExecuteAsync(WsRequestDto request, string socketId)
	{
		if (request.Payload == null)
			throw new ArgumentNullException(nameof(request));

		var payload = JsonSerializer.Deserialize<JoinLeavePayload>(request.Payload.Value.GetRawText(), options) ?? throw new ArgumentException("Invalid join payload");
		await channelCommandProcessor.JoinChannel(payload.ChannelId, payload.UserId);
	}
}