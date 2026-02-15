using System.Text.Json;

using RealTimeChatServer.Websockets.Interfaces;
using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Websockets.Commands;

public class LoginUserCommand(IUserCommandProcessor userCommandProcessor, JsonSerializerOptions options) : IWebSocketCommand
{
	public string Name => "loginUser";

	public async Task ExecuteAsync(WsRequestDto request, string socketId)
	{
		if (request.Payload == null)
			throw new ArgumentNullException(nameof(request));

		var payload = JsonSerializer.Deserialize<LoginUserPayload>(request.Payload.Value.GetRawText(), options) ?? throw new ArgumentException("Invalid login payload");
		var user = new Models.User
		{
			Name = payload.Username,
			Password = payload.Password
		};

		await userCommandProcessor.Login(user, socketId);
	}
}