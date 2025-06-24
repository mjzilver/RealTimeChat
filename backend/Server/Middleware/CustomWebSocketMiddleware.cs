using System.Net;
using System.Net.WebSockets;
using System.Text.Json;

using RealTimeChatServer.Data;
using RealTimeChatServer.Websockets;
using RealTimeChatServer.Websockets.Interfaces;

namespace RealTimeChatServer.Middleware;
public class CustomWebSocketMiddleware(
	RequestDelegate next,
	IWebSocketCommandProcessor commandProcessor,
	IMemoryStore memoryStore,
	JsonSerializerOptions options)
{
	private readonly RequestDelegate _next = next;
	private readonly IWebSocketCommandProcessor _commandProcessor = commandProcessor;
	private readonly IMemoryStore _memoryStore = memoryStore;
	private readonly JsonSerializerOptions _options = options;

	public async Task InvokeAsync(HttpContext context)
	{
		if (context.Request.Path == "/ws")
		{
			if (context.WebSockets.IsWebSocketRequest)
			{
				WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
				WebSocketHandler handler = new(webSocket, _commandProcessor, _memoryStore, _options);
				await handler.Handle();
			}
			else
			{
				context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
			}
		}

		await _next(context);
	}
}
