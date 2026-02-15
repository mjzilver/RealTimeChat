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
		var type = request.Type;
		if (string.IsNullOrWhiteSpace(type))
		{
			Console.WriteLine("Received request with empty Type");
			return;
		}

		if (_commands.TryGetValue(type, out var handler))
		{
			await handler.ExecuteAsync(request, socketId);
		}
		else
		{
			Console.WriteLine($"Unknown command/type: {type}");
		}
	}

	public async Task HandleDisconnectionAsync(string socketId)
	{
		await _userCommandProcessor.Logout(socketId);
	}
}
