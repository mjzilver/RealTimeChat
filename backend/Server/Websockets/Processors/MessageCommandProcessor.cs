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
		var wsMessages = messages.Select(m => new Payloads.MessagePayload(
			m.Id,
			m.UserId,
			m.ChannelId,
			m.Text,
			m.Time,
			new Payloads.UserPayload(m.User.Id, m.User.Name, m.User.Color, m.User.Joined)
		)).ToList();

		var envelope = new { type = "messages", payload = new { messages = wsMessages } };
		var response = JsonSerializer.Serialize(envelope, options);
		var socket = memoryStore.GetSocketById(socketId);
		if (socket != null)
		{
			await webSocketSender.SendAsync(socket, response);
		}
	}

	public async Task BroadcastMessage(Message message, string socketId)
	{
		var user = await dbContext.Users.FindAsync(message.UserId);
		var channel = await dbContext.Channels.FindAsync(message.ChannelId);

		if (user == null || channel == null)
		{
			await webSocketSender.SendErrorAsync(socketId, "User or channel not found");
			return;
		}

		message.User = user;
		message.Channel = channel;

		(var IsValid, var ErrorMessage) = MessageValidator.ValidateNewMessage(message);

		if (!IsValid)
		{
			await webSocketSender.SendErrorAsync(socketId, ErrorMessage);
			return;
		}

		message.Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

		var sockets = memoryStore.GetSocketsInChannel(message.ChannelId);
		var msgPayload = new Payloads.MessagePayload(
			message.Id,
			message.UserId,
			message.ChannelId,
			message.Text,
			message.Time,
			new Payloads.UserPayload(message.User.Id, message.User.Name, message.User.Color, message.User.Joined)
		);
		var envelope = new { type = "broadcast", payload = new { message = msgPayload } };
		var serialized = JsonSerializer.Serialize(envelope, options);
		foreach (var socket in sockets)
		{
			await webSocketSender.SendAsync(socket, serialized);
		}

		await dbContext.Messages.AddAsync(message);
		await dbContext.SaveChangesAsync();
	}
}
