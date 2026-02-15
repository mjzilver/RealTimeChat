using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

using RealTimeChatServer.Data;
using RealTimeChatServer.Websockets.Interfaces;

namespace RealTimeChatServer.Websockets;

public class WebSocketSender(IMemoryStore memoryStore, JsonSerializerOptions options) : IWebSocketSender
{
	private readonly IMemoryStore _memoryStore = memoryStore;
	private readonly JsonSerializerOptions _options = options;

	public async Task SendAsync(WebSocket socket, string message)
	{
		if (socket.State == WebSocketState.Open)
		{
			var pool = ArrayPool<byte>.Shared;
			var needed = Encoding.UTF8.GetByteCount(message);
			var buffer = pool.Rent(needed);
			try
			{
				var written = Encoding.UTF8.GetBytes(message, 0, message.Length, buffer, 0);
				await socket.SendAsync(new ArraySegment<byte>(buffer, 0, written), WebSocketMessageType.Text, true, CancellationToken.None);
			}
			finally
			{
				try { pool.Return(buffer); } catch { }
			}
		}
	}

	public async Task SendErrorAsync(string socketId, string errorMessage)
	{
		var envelope = new { type = "error", payload = new { error = new { message = errorMessage } } };
		var response = JsonSerializer.Serialize(envelope, _options);
		var socket = _memoryStore.GetSocketById(socketId);
		if (socket != null)
		{
			await SendAsync(socket, response);
		}
	}
}
