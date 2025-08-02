using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using RealTimeChatServer.Data;
using RealTimeChatServer.Models;
using RealTimeChatServer.Validators;
using RealTimeChatServer.Websockets.Interfaces;

namespace RealTimeChatServer.Websockets.Processors;

public class MessageCommandProcessor(
	AppDbContext dbContext,
	IMemoryStore memoryStore,
	IWebSocketSender webSocketSender,
	JsonSerializerOptions options
) : IMessageCommandProcessor
{
	public async Task GetMessages(int channelId, string socketId)
	{
		var messages = await dbContext.Messages
			.Include(m => m.User)
			.Include(m => m.Channel)
			.Where(m => m.ChannelId == channelId)
			.ToListAsync();
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

		(var IsValid, var ErrorMessage) = MessageValidator.ValidateNewMessage(message);

		if (!IsValid)
		{
			await webSocketSender.SendErrorAsync(socketId, ErrorMessage);
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
