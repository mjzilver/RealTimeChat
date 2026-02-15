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
	public async Task Handle(CancellationToken cancellationToken)
	{
		var socketId = Guid.NewGuid().ToString();

			try
			{
				memoryStore.AddSocketConnection(socketId, webSocket);

				var arrayPool = System.Buffers.ArrayPool<byte>.Shared;
				var rentedBuffer = arrayPool.Rent(1024 * 4);
				var buffer = new ArraySegment<byte>(rentedBuffer);
				var ms = new MemoryStream();

				try
				{
					while (webSocket.State == WebSocketState.Open)
					{
						using var loopMs = new MemoryStream();
						WebSocketReceiveResult? result;
						do
						{
							result = await webSocket.ReceiveAsync(buffer, cancellationToken);
							if (result.MessageType == WebSocketMessageType.Close)
							{
								await commandDispatcher.HandleDisconnectionAsync(socketId);
								try { await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", cancellationToken); } catch { }
								break;
							}

							loopMs.Write(rentedBuffer, 0, result.Count);
						} while (!result.EndOfMessage);

						if (result != null && result.MessageType == WebSocketMessageType.Text)
						{
							loopMs.Seek(0, SeekOrigin.Begin);
							using var reader = new StreamReader(loopMs, Encoding.UTF8);
							var json = await reader.ReadToEndAsync(cancellationToken);
							var command = JsonSerializer.Deserialize<WsRequestDto>(json, options);
							if (command != null)
							{
								await commandDispatcher.ProcessCommandAsync(command, socketId);
							}
						}
					}
				}
				finally
				{
					try { ms.Dispose(); } catch { }
					try { arrayPool.Return(rentedBuffer); } catch { }
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
