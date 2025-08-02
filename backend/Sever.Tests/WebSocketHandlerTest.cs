using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Moq;

using RealTimeChatServer.Data;
using RealTimeChatServer.Websockets;
using RealTimeChatServer.Websockets.Interfaces;
using RealTimeChatServer.Websockets.Payloads;

namespace Sever.Tests
{
	[TestClass]
	public class WebSocketHandlerTests
	{
		private Mock<WebSocket> _mockWebSocket = null!;
		private Mock<IWebSocketCommandDispatcher> _mockDispatcher = null!;
		private Mock<IMemoryStore> _mockMemoryStore = null!;
		private readonly JsonSerializerOptions _options = new()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			ReferenceHandler = ReferenceHandler.IgnoreCycles
		};

		private WebSocketHandler _webSocketHandler = null!;

		[TestInitialize]
		public void Setup()
		{
			_mockWebSocket = new Mock<WebSocket>();
			_mockDispatcher = new Mock<IWebSocketCommandDispatcher>();
			_mockMemoryStore = new Mock<IMemoryStore>();
			_webSocketHandler = new WebSocketHandler(_mockWebSocket.Object, _mockDispatcher.Object, _mockMemoryStore.Object, _options);

			_mockWebSocket.SetupGet(w => w.State).Returns(WebSocketState.Open);

			var buffer = Encoding.UTF8.GetBytes("{\"command\":\"test\"}");
			var stream = new MemoryStream(buffer);

			WebSocketReceiveResult result = new(buffer.Length, WebSocketMessageType.Text, true);

			_mockWebSocket.Setup(w => w.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(result)
				.Callback((ArraySegment<byte> buffer, CancellationToken token)
				=> stream.Read(buffer.Array!, buffer.Offset, buffer.Count));
		}

		[TestMethod]
		public async Task Handle_ShouldAddSocketConnectionToMemoryStore()
		{
			// Act
			await _webSocketHandler.Handle();

			// Assert
			_mockMemoryStore.Verify(m => m.AddSocketConnection(It.IsAny<string>(), _mockWebSocket.Object), Times.Once);
		}

		[TestMethod]
		public async Task Handle_ShouldProcessCommandAsync()
		{
			// Act
			await _webSocketHandler.Handle();

			// Assert
			_mockDispatcher.Verify(c => c.ProcessCommandAsync(It.IsAny<WsRequestDto>(), It.IsAny<string>()), Times.Once);
		}

		[TestMethod]
		public async Task Handle_ShouldRemoveSocketConnectionFromMemoryStoreOnClose()
		{
			// Arrange
			_mockWebSocket.SetupGet(w => w.State).Returns(WebSocketState.CloseReceived);

			// Act
			await _webSocketHandler.Handle();

			// Assert
			_mockMemoryStore.Verify(m => m.RemoveSocketConnection(It.IsAny<string>()), Times.Once);
		}

		[TestMethod]
		public async Task Handle_ShouldCloseWebSocketOnClientClose()
		{
			// Arrange
			WebSocketReceiveResult closeResult = new(0, WebSocketMessageType.Close, true);

			_mockWebSocket.SetupSequence(w => w.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(closeResult);

			_mockWebSocket.SetupSequence(w => w.State)
				.Returns(WebSocketState.Open)
				.Returns(WebSocketState.CloseReceived);

			// Act
			await _webSocketHandler.Handle();

			// Assert
			_mockDispatcher.Verify(c => c.HandleDisconnectionAsync(It.IsAny<string>()), Times.Once);
			_mockWebSocket.Verify(w => w.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task Handle_ShouldCatchExceptions()
		{
			// Arrange
			_mockDispatcher.Setup(c => c.ProcessCommandAsync(It.IsAny<WsRequestDto>(), It.IsAny<string>()))
				.ThrowsAsync(new Exception("Test exception"));

			// Act
			await _webSocketHandler.Handle();

			// Assert
			_mockMemoryStore.Verify(m => m.RemoveSocketConnection(It.IsAny<string>()), Times.Once);
		}
	}
}
