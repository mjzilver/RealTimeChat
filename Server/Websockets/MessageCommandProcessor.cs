using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using B4mServer.Models;
using B4mServer.Data;
using B4mServer.Validators;
using B4mServer.Websockets.Interfaces;

namespace B4mServer.Websockets;

public class MessageCommandProcessor(AppDbContext dbContext, IMemoryStore memoryStore,
    IWebSocketSender webSocketSender, JsonSerializerOptions options) : IMessageCommandProcessor
{
    public async Task GetMessages(int channelId, string socketId)
	{
		var messages = await dbContext.Messages.Include(m => m.User).Include(m => m.Channel).Where(m => m.ChannelId == channelId).ToListAsync();
		var wsMessages = messages.Select(m => m.ToDTO()).ToList();
		var response = JsonSerializer.Serialize(new { command = "messages", messages = wsMessages }, options);
		var socket = memoryStore.GetSocketById(socketId);
		if (socket != null)
		{
			await webSocketSender.SendAsync(socket, response);
		}
	}

	public async Task BroadcastMessage(Message message, string socketId)
	{
		message.User = await dbContext.Users.FindAsync(message.UserId) ?? new User();
		message.Channel = await dbContext.Channels.FindAsync(message.ChannelId) ?? new Channel();

		if (message.User == null || message.Channel == null)
		{
			return;
		}

		var validation = MessageValidator.ValidateNewMessage(message);

		if(!validation.IsValid)
		{
			await webSocketSender.SendErrorAsync(socketId, validation.ErrorMessage);
			return;
		}

		message.Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

		foreach (var socket in memoryStore.GetAllSockets())
		{
			await webSocketSender.SendAsync(socket, JsonSerializer.Serialize(new { command = "broadcast", message }, options));
		}

		await dbContext.Messages.AddAsync(message);
		await dbContext.SaveChangesAsync();
	}
}
