using System.Text.Json;

using RealTimeChatServer.Websockets.Interfaces;
using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Websockets.Commands;

public class UpdateChannelCommand(IChannelCommandProcessor channelCommandProcessor, JsonSerializerOptions options) : IWebSocketCommand
{
	public string Name => "updateChannel";

	public async Task ExecuteAsync(WsRequestDto request, string socketId)
	{
		if (request.Payload == null)
			throw new ArgumentNullException(nameof(request));

		var payload = System.Text.Json.JsonSerializer.Deserialize<UpdateChannelPayload>(request.Payload.Value.GetRawText(), options) ?? throw new ArgumentException("Invalid update channel payload");

		var channel = new Models.Channel
		{
			Id = payload.Id,
			Name = payload.Name,
			Password = payload.Password,
			Color = payload.Color,
			OwnerId = payload.OwnerId
		};

		await channelCommandProcessor.UpdateChannel(channel, socketId);
	}
}