using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using B4mServer.Data;
using B4mServer.Websockets.Interfaces;

namespace B4mServer.Websockets;

public class WebSocketSender(IMemoryStore memoryStore, JsonSerializerOptions options) : IWebSocketSender
{
	private readonly IMemoryStore _memoryStore = memoryStore;
	private readonly JsonSerializerOptions _options = options;

    public async Task SendAsync(WebSocket socket, string message)
	{
		if (socket.State == WebSocketState.Open)
		{
			var encodedMessage = Encoding.UTF8.GetBytes(message);
			await socket.SendAsync(new ArraySegment<byte>(encodedMessage), WebSocketMessageType.Text, true, CancellationToken.None);
		}
	}

	public async Task SendErrorAsync(string socketId, string errorMessage)
	{
		var response = JsonSerializer.Serialize(new { command = "error", error = errorMessage }, _options);
		var socket = _memoryStore.GetSocketById(socketId);
		if (socket != null)
		{
			await SendAsync(socket, response);
		}
	}
}
