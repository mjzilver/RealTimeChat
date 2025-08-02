using System.Reflection;

using RealTimeChatServer.Websockets.Interfaces;
using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Websockets.Dispatcher;

public class WebSocketCommandDispatcher(IServiceProvider serviceProvider, IUserCommandProcessor userCommandProcessor) : IWebSocketCommandDispatcher
{
	private readonly Dictionary<string, IWebSocketCommand> _commands = Assembly.GetExecutingAssembly()
			.GetTypes()
			.Where(t => typeof(IWebSocketCommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
			.Select(t => (IWebSocketCommand)ActivatorUtilities.CreateInstance(serviceProvider, t))
			.ToDictionary(cmd => cmd.Name, StringComparer.OrdinalIgnoreCase);
	private readonly IUserCommandProcessor _userCommandProcessor = userCommandProcessor;

	public async Task ProcessCommandAsync(WsRequestDto request, string socketId)
	{
		if (_commands.TryGetValue(request.Command, out var handler))
		{
			await handler.ExecuteAsync(request, socketId);
		}
		else
		{
			Console.WriteLine($"Unknown command: {request.Command}");
		}
	}

	public async Task HandleDisconnectionAsync(string socketId)
	{
		await _userCommandProcessor.Logout(socketId);
	}
}
