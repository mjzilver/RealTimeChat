using Microsoft.AspNetCore.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace B4mServer.Websockets;

public class WebSocketSender
{
	private readonly MemoryStore _memoryStore;
	private readonly JsonSerializerOptions _options;
    public WebSocketSender(MemoryStore memoryStore, JsonSerializerOptions options)
    {
        _memoryStore = memoryStore;
		_options = options;
    }

    public async Task SendAsync(WebSocket socket, string message)
	{
		Console.WriteLine("Sending message" + message);
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
