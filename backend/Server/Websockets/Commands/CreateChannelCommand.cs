using System.Text.Json;

using RealTimeChatServer.Websockets.Interfaces;
using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Websockets.Commands;

public class CreateChannelCommand(IChannelCommandProcessor channelCommandProcessor, JsonSerializerOptions options) : IWebSocketCommand
{
	public string Name => "createChannel";

	public async Task ExecuteAsync(WsRequestDto request, string socketId)
	{
		if (request.Payload == null)
			throw new ArgumentNullException(nameof(request));

		var payload = JsonSerializer.Deserialize<CreateChannelPayload>(request.Payload.Value.GetRawText(), options);
		if (payload == null)
			throw new ArgumentException("Invalid channel payload");

		var channel = new Models.Channel
		{
			Name = payload.Name,
			Password = payload.Password,
			Color = payload.Color,
			OwnerId = payload.OwnerId
		};

		await channelCommandProcessor.CreateChannel(channel, socketId);
	}
}