using RealTimeChatServer.Websockets.Interfaces;
using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Websockets.Commands;

public class GetUsersCommand(IUserCommandProcessor userCommandProcessor) : IWebSocketCommand
{
	public string Name => "getUsers";

	public async Task ExecuteAsync(WsRequestDto request, string socketId)
	{
		await userCommandProcessor.GetUsers(socketId);
	}
}