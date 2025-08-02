using RealTimeChatServer.Websockets.Interfaces;
using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Websockets.Commands;

public class LoginUserCommand(IUserCommandProcessor userCommandProcessor) : IWebSocketCommand
{
	public string Name => "loginUser";

	public async Task ExecuteAsync(WsRequestDto request, string socketId)
	{
		if (request.User == null)
			throw new ArgumentNullException(nameof(request.User));

		await userCommandProcessor.Login(request.User.ToModel(), socketId);
	}
}