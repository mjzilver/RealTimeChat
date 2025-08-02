using System.Net;
using System.Text.Json;

using RealTimeChatServer.Data;
using RealTimeChatServer.Websockets;
using RealTimeChatServer.Websockets.Interfaces;

namespace RealTimeChatServer.Middleware;
public class ChatWebSocketMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
{
    private readonly RequestDelegate _next = next;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path == "/ws")
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                using var scope = _serviceProvider.CreateScope();
                var dispatcher = scope.ServiceProvider.GetRequiredService<IWebSocketCommandDispatcher>();
                var memoryStore = scope.ServiceProvider.GetRequiredService<IMemoryStore>();
                var options = scope.ServiceProvider.GetRequiredService<JsonSerializerOptions>();

                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var handler = new WebSocketHandler(webSocket, dispatcher, memoryStore, options);

                await handler.Handle();
                return;
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }
        }

        await _next(context);
    }
}
