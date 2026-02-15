using System.Text.Json;

using RealTimeChatServer.Websockets.Interfaces;
using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Websockets.Commands;

public class LeaveChannelCommand(IChannelCommandProcessor channelCommandProcessor, JsonSerializerOptions options) : IWebSocketCommand
{
	public string Name => "leaveChannel";

	public async Task ExecuteAsync(WsRequestDto request, string socketId)
	{
		if (request.Payload == null)
			throw new ArgumentNullException(nameof(request));

		var payload = JsonSerializer.Deserialize<JoinLeavePayload>(request.Payload.Value.GetRawText(), options) ?? throw new ArgumentException("Invalid leave payload");
		await channelCommandProcessor.LeaveChannel(payload.ChannelId, payload.UserId);
	}
}