using RealTimeChatServer.Websockets.Interfaces;
using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Websockets.Commands;

public class RegisterUserCommand(IUserCommandProcessor userCommandProcessor) : IWebSocketCommand
{
	public string Name => "registerUser";

	public async Task ExecuteAsync(WsRequestDto request, string socketId)
	{
		if (request.User == null)
			throw new ArgumentNullException(nameof(request.User));

		await userCommandProcessor.Register(request.User.ToModel(), socketId);
	}
}
