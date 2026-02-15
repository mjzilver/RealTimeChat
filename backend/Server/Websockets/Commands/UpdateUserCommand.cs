using System.Text.Json;

using RealTimeChatServer.Websockets.Interfaces;
using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Websockets.Commands;

public class UpdateUserCommand(IUserCommandProcessor userCommandProcessor, JsonSerializerOptions options) : IWebSocketCommand
{
	public string Name => "updateUser";

	public async Task ExecuteAsync(WsRequestDto request, string socketId)
	{
		if (request.Payload == null)
			throw new ArgumentNullException(nameof(request));

		var payload = JsonSerializer.Deserialize<UpdateUserPayload>(request.Payload.Value.GetRawText(), options) ?? throw new ArgumentException("Invalid update user payload");

		var user = new Models.User
		{
			Id = payload.Id,
			Name = payload.Name,
			Color = payload.Color
		};

		await userCommandProcessor.UpdateUser(user, socketId);
	}
}
