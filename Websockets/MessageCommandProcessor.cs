using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using B4mServer.Models;
using B4mServer.Data;
using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Options;

namespace B4mServer.Websockets
{
	public class MessageCommandProcessor
	{
		private readonly AppDbContext _dbContext;
		private readonly MemoryStore _memoryStore;
		private readonly JsonSerializerOptions _options;
		private readonly WebSocketSender _webSocketSender;

		public MessageCommandProcessor(AppDbContext dbContext, MemoryStore memoryStore,
			WebSocketSender webSocketSender, JsonSerializerOptions options)
		{
			_dbContext = dbContext;
			_memoryStore = memoryStore;
			_options = options;
			_webSocketSender = webSocketSender;
		}

		public async Task GetMessages(int channelId, string socketId)
		{
			var messages = await _dbContext.Messages.Include(m => m.User).Include(m => m.Channel).Where(m => m.ChannelId == channelId).ToListAsync();
			var wsMessages = messages.Select(m => m.ToDTO()).ToList();
			var response = JsonSerializer.Serialize(new { command = "messages", messages = wsMessages }, _options);
			var socket = _memoryStore.GetSocketById(socketId);
			if (socket != null)
			{
				await _webSocketSender.SendAsync(socket, response);
			}
		}

		public async Task BroadcastMessage(Message message)
		{
			Console.WriteLine(JsonSerializer.Serialize(message, _options));

			message.User = await _dbContext.Users.FindAsync(message.UserId) ?? new User();
			message.Channel = await _dbContext.Channels.FindAsync(message.ChannelId) ?? new Channel();

			if (message.User == null || message.Channel == null)
			{
				return;
			}

			foreach (var socket in _memoryStore.GetAllSockets())
			{
				await _webSocketSender.SendAsync(socket, JsonSerializer.Serialize(new { command = "broadcast", message }, _options));
			}

			await _dbContext.Messages.AddAsync(message);
			await _dbContext.SaveChangesAsync();
		}
	}
}
