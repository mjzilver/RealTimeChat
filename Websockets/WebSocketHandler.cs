using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using B4mServer.Data;

namespace B4mServer.Websockets;

public class WebSocketHandler
{
	private readonly HttpContext _context;
	private readonly WebSocket _webSocket;
	private readonly WebSocketCommandProcessor _commandProcessor;
	private readonly MemoryStore _memoryStore;
	private readonly JsonSerializerOptions _options;

	public WebSocketHandler(HttpContext context, WebSocket webSocket, WebSocketCommandProcessor commandProcessor,
		MemoryStore memoryStore, JsonSerializerOptions options)
	{
		_context = context;
		_webSocket = webSocket;
		_commandProcessor = commandProcessor;
		_memoryStore = memoryStore;
		_options = options;
	}

	public async Task Handle()
	{
		string socketId = Guid.NewGuid().ToString();
		_memoryStore.AddSocketConnection(socketId, _webSocket);

		while (_webSocket.State == WebSocketState.Open)
		{
			var buffer = new byte[1024 * 4];
			var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

			if (result.MessageType == WebSocketMessageType.Text)
			{
				var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
				var command = JsonSerializer.Deserialize<WebSocketCommand>(json, _options);

				if (command != null)
				{
					await _commandProcessor.ProcessCommandAsync(command, socketId);
				}
			}
			else if (result.MessageType == WebSocketMessageType.Close)
			{
				_commandProcessor.UserDisconnected(socketId);
				
				await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
			}
		}
	}
}
