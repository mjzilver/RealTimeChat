using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using RealTimeChatServer.Data;
using RealTimeChatServer.Models;
using RealTimeChatServer.Validators;
using RealTimeChatServer.Websockets.Interfaces;
using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Websockets.Processors;

public class ChannelCommandProcessor(
	AppDbContext dbContext,
	IMemoryStore memoryStore,
	IWebSocketSender webSocketSender,
	JsonSerializerOptions options
) : IChannelCommandProcessor
{
	public async Task GetChannels(string socketId)
	{
		var channels = await dbContext.Channels.Select(c => new ChannelPayload(c.Id, c.Name, c.Color, c.Created, null, c.OwnerId, null, null)).ToListAsync();

		var updatedChannels = new List<ChannelPayload>();
		foreach (var channel in channels)
		{
			var users = memoryStore.GetUsersInChannel(channel.Id);
			var wsUsers = users.Select(u => new UserPayload(u.Id, u.Name, u.Color, u.Joined)).ToList();

			var ch = new ChannelPayload(channel.Id, channel.Name, channel.Color, channel.Created, channel.Password, channel.OwnerId, wsUsers);
			updatedChannels.Add(ch);
		}
		var response = JsonSerializer.Serialize(new { type = "channels", payload = new { channels = updatedChannels } }, options);
		var socket = memoryStore.GetSocketById(socketId);
		if (socket != null)
		{
			await webSocketSender.SendAsync(socket, response);
		}
	}

	public async Task CreateChannel(Channel channel, string socketId)
	{
		var currentUser = memoryStore.GetUserBySocketId(socketId);

		if (currentUser == null)
		{
			await webSocketSender.SendErrorAsync(socketId, "Unauthorized");
			return;
		}

		(var IsValid, var ErrorMessage) = ChannelValidator.ValidateNewChannel(channel);

		Console.WriteLine(ErrorMessage);

		if (!IsValid)
		{
			await webSocketSender.SendErrorAsync(socketId, ErrorMessage);
			return;
		}

		channel.Owner = currentUser;
		channel.Created = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

		await dbContext.Channels.AddAsync(channel);
		await dbContext.SaveChangesAsync();

		var chPayload = new ChannelPayload(channel.Id, channel.Name, channel.Color, channel.Created, channel.Password, channel.OwnerId);
		var response = JsonSerializer.Serialize(new { type = "channelCreated", payload = new { channel = chPayload } }, options);
		foreach (var socket in memoryStore.GetAllSockets())
		{
			await webSocketSender.SendAsync(socket, response);
		}
	}
	public async Task UpdateChannel(Channel channel, string socketId)
	{
		var currentChannel = await dbContext.Channels.FirstOrDefaultAsync(c => c.Id == channel.Id);
		var owner = memoryStore.GetUserBySocketId(socketId);

		if (currentChannel == null)
		{
			await webSocketSender.SendErrorAsync(socketId, "Channel does not exist");
			return;
		}

		if (owner == null || owner.Id != currentChannel.Owner!.Id)
		{
			await webSocketSender.SendErrorAsync(socketId, "You do not own this channel");
			return;
		}

		currentChannel.Name = channel.Name;
		currentChannel.Color = channel.Color;
		currentChannel.Password = channel.Password;
		currentChannel.Owner = owner;
		currentChannel.OwnerId = channel.OwnerId;

		await dbContext.SaveChangesAsync();

		var chPayload = new ChannelPayload(currentChannel.Id, currentChannel.Name, currentChannel.Color, currentChannel.Created, currentChannel.Password, currentChannel.OwnerId);
		var response = JsonSerializer.Serialize(new { type = "channelUpdated", payload = new { channel = chPayload } }, options);
		foreach (var socket in memoryStore.GetAllSockets())
		{
			await webSocketSender.SendAsync(socket, response);
		}
	}

	public async Task DeleteChannel(int channelId, string socketId)
	{
		var channel = await dbContext.Channels.FindAsync(channelId);
		var owner = memoryStore.GetUserBySocketId(socketId);
		var currentUser = memoryStore.GetUserBySocketId(socketId);

		if (channel != null && currentUser != null)
		{
			if (owner == null || owner.Id != currentUser.Id)
			{
				await webSocketSender.SendErrorAsync(socketId, "You do not own this channel");
				return;
			}

			var chPayload = new ChannelPayload(channel.Id, channel.Name, channel.Color, channel.Created, channel.Password, channel.OwnerId);
			var response = JsonSerializer.Serialize(new { type = "channelDeleted", payload = new { channel = chPayload } }, options);

			dbContext.Channels.Remove(channel);
			await dbContext.SaveChangesAsync();

			var socket = memoryStore.GetSocketById(socketId);
			if (socket != null)
			{
				await webSocketSender.SendAsync(socket, response);
			}
		}
	}

	public async Task JoinChannel(int channelId, int userId)
	{
		var channel = await dbContext.Channels.FirstOrDefaultAsync(c => c.Id == channelId);
		var user = await dbContext.Users.FindAsync(userId);

		if (channel != null && user != null)
		{
			memoryStore.AddUserToChannel(channelId, user);
			var users = memoryStore.GetUsersInChannel(channelId);
			var wsUsers = users.Select(u => new UserPayload(u.Id, u.Name, u.Color, u.Joined)).ToList();

			var wsChannel = new ChannelPayload(channel.Id, channel.Name, channel.Color, channel.Created, channel.Password, channel.OwnerId, wsUsers);

			var response = JsonSerializer.Serialize(new { type = "userJoinedChannel", payload = new { channel = wsChannel } }, options);
			foreach (var socket in memoryStore.GetAllSockets())
			{
				await webSocketSender.SendAsync(socket, response);
			}
		}
	}

	public async Task LeaveChannel(int channelId, int userId)
	{
		var channel = await dbContext.Channels.FirstOrDefaultAsync(c => c.Id == channelId);
		var user = await dbContext.Users.FindAsync(userId);

		if (channel != null && user != null)
		{
			memoryStore.RemoveUserFromChannel(channelId, user);
			var users = memoryStore.GetUsersInChannel(channelId);
			var wsUsers = users.Select(u => new UserPayload(u.Id, u.Name, u.Color, u.Joined)).ToList();

			var wsChannel = new ChannelPayload(channel.Id, channel.Name, channel.Color, channel.Created, channel.Password, channel.OwnerId, wsUsers);

			var response = JsonSerializer.Serialize(new { type = "userLeftChannel", payload = new { channel = wsChannel } }, options);
			foreach (var socket in memoryStore.GetAllSockets())
			{
				await webSocketSender.SendAsync(socket, response);
			}
		}
	}
}
