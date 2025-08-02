using RealTimeChatServer.Websockets.Interfaces;
using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Websockets.Commands;

public class UpdateUserCommand(IUserCommandProcessor userCommandProcessor) : IWebSocketCommand
{
	public string Name => "updateUser";

	public async Task ExecuteAsync(WsRequestDto request, string socketId)
	{
		if (request.User == null)
			throw new ArgumentNullException(nameof(request.User));

		await userCommandProcessor.UpdateUser(request.User.ToModel(), socketId);
	}
}
