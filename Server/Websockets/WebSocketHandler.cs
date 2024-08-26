using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using B4mServer.Data;
using B4mServer.Websockets.Interfaces;

namespace B4mServer.Websockets;

public class WebSocketHandler(
	WebSocket webSocket,
	IWebSocketCommandProcessor commandProcessor,
	IMemoryStore memoryStore,
	JsonSerializerOptions options
) : IWebSocketHandler
{
	public async Task Handle()
	{
		string socketId = Guid.NewGuid().ToString();

		try
		{
			memoryStore.AddSocketConnection(socketId, webSocket);

			while (webSocket.State == WebSocketState.Open)
			{
				var buffer = new byte[1024 * 4];
				var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
	
				if (result.MessageType == WebSocketMessageType.Text)
				{
					var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
					var command = JsonSerializer.Deserialize<WebSocketCommand>(json, options);

					if (command != null)
					{
						await commandProcessor.ProcessCommandAsync(command, socketId);
					}
				}
				else if (result.MessageType == WebSocketMessageType.Close)
				{
					commandProcessor.UserDisconnected(socketId);

					await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
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
