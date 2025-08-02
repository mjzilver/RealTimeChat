using RealTimeChatServer.Websockets.Interfaces;
using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Websockets.Commands;

public class LogoutUserCommand(IUserCommandProcessor userCommandProcessor) : IWebSocketCommand
{
	public string Name => "logoutUser";

	public async Task ExecuteAsync(WsRequestDto request, string socketId)
	{
		await userCommandProcessor.Logout(socketId);
	}
}
