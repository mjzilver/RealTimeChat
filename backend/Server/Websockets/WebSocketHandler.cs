using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

using RealTimeChatServer.Data;
using RealTimeChatServer.Websockets.Interfaces;
using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Websockets;

public class WebSocketHandler(
	WebSocket webSocket,
	IWebSocketCommandDispatcher commandDispatcher,
	IMemoryStore memoryStore,
	JsonSerializerOptions options
) : IWebSocketHandler
{
	public async Task Handle()
	{
		var socketId = Guid.NewGuid().ToString();

		try
		{
			memoryStore.AddSocketConnection(socketId, webSocket);

			while (webSocket.State == WebSocketState.Open)
			{
				var buffer = new ArraySegment<byte>(new byte[1024 * 4]);
				using var ms = new MemoryStream();
				WebSocketReceiveResult? result;
				do
				{
					result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
					if (result.MessageType == WebSocketMessageType.Close)
					{
						await commandDispatcher.HandleDisconnectionAsync(socketId);
						await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
						break;
					}

					ms.Write(buffer.Array!, buffer.Offset, result.Count);
				} while (!result.EndOfMessage);

				if (result != null && result.MessageType == WebSocketMessageType.Text)
				{
					ms.Seek(0, SeekOrigin.Begin);
					using var reader = new StreamReader(ms, Encoding.UTF8);
					var json = await reader.ReadToEndAsync();
					var command = JsonSerializer.Deserialize<WsRequestDto>(json, options);
					if (command != null)
					{
						await commandDispatcher.ProcessCommandAsync(command, socketId);
					}
				}
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error: {ex.Message}\n{ex.StackTrace}");
		}
		finally
		{
			memoryStore.RemoveSocketConnection(socketId);
		}
	}
}
