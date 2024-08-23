using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using B4mServer.Models;
using B4mServer.Data;
using B4mServer.Validators;
using B4mServer.Websockets.Interfaces;

namespace B4mServer.Websockets;

public class ChannelCommandProcessor(AppDbContext dbContext, IMemoryStore memoryStore,
    IWebSocketSender webSocketSender, JsonSerializerOptions options) : IChannelCommandProcessor
{
    public async Task GetChannels(string socketId)
	{
		var channels = await dbContext.Channels.Select(c => new WebSocketChannel
		{
			Id = c.Id,
			Name = c.Name,
			Created = c.Created,
			Color = c.Color,
			OwnerId = c.OwnerId,
			Password = null,
		}).ToListAsync();

		// add users to channels
		foreach (var channel in channels)
		{
			var users = memoryStore.GetUsersInChannel(channel.Id ?? -1);
			channel.Users = users.Select(u => new WebSocketUser
			{
				Id = u.Id,
				Name = u.Name,
				Color = u.Color
			}).ToList();
		}

		var response = JsonSerializer.Serialize(new { command = "channels", channels }, options);
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

		var channelValidation = ChannelValidator.ValidateNewChannel(channel);

		Console.WriteLine(channelValidation.ErrorMessage);

		if(!channelValidation.IsValid)
		{
			await webSocketSender.SendErrorAsync(socketId, channelValidation.ErrorMessage);
			return;
		}

		channel.Owner = currentUser;
		channel.Created = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

		await dbContext.Channels.AddAsync(channel);
		await dbContext.SaveChangesAsync();

		var response = JsonSerializer.Serialize(new { command = "channelCreated", channel }, options);
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
			await webSocketSender.SendErrorAsync(socketId, "Channel does not exit");
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

		var response = JsonSerializer.Serialize(new { command = "channelUpdated", channel = currentChannel }, options);
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

			var response = JsonSerializer.Serialize(new { command = "channelDeleted", channel = channel.ToDTO() }, options);

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
			// serialze users
			var wsUsers = users.Select(u => u.ToDTO()).ToList();

			WebSocketChannel wsChannel = channel.ToDTO();
			wsChannel.Users = wsUsers;

			var response = JsonSerializer.Serialize(new { command = "userJoinedChannel", channel = wsChannel }, options);
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
			var wsUsers = users.Select(u => u.ToDTO()).ToList();

			WebSocketChannel wsChannel = channel.ToDTO();
			wsChannel.Users = wsUsers;

			var response = JsonSerializer.Serialize(new { command = "userLeftChannel", channel = wsChannel }, options);
			foreach (var socket in memoryStore.GetAllSockets())
			{
				await webSocketSender.SendAsync(socket, response);
			}
		}
	}
}
