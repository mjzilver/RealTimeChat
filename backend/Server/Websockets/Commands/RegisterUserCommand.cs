using System.Text.Json;

using RealTimeChatServer.Websockets.Interfaces;
using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Websockets.Commands;

public class RegisterUserCommand(IUserCommandProcessor userCommandProcessor, JsonSerializerOptions options) : IWebSocketCommand
{
	public string Name => "registerUser";

	public async Task ExecuteAsync(WsRequestDto request, string socketId)
	{
		if (request.Payload == null)
			throw new ArgumentNullException(nameof(request));

		var payload = JsonSerializer.Deserialize<RegisterUserPayload>(request.Payload.Value.GetRawText(), options) ?? throw new ArgumentException("Invalid register payload");
		var user = new Models.User
		{
			Name = payload.Username,
			Password = payload.Password,
			Color = payload.Color ?? "black"
		};

		await userCommandProcessor.Register(user, socketId);
	}
}
