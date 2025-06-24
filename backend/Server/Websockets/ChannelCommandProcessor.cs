using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using RealTimeChatServer.Data;
using RealTimeChatServer.Models;
using RealTimeChatServer.Validators;
using RealTimeChatServer.Websockets.Interfaces;

namespace RealTimeChatServer.Websockets;

public class ChannelCommandProcessor(AppDbContext dbContext, IMemoryStore memoryStore,
	IWebSocketSender webSocketSender, JsonSerializerOptions options) : IChannelCommandProcessor
{
	public async Task GetChannels(string socketId)
	{
		List<WebSocketChannel> channels = await dbContext.Channels.Select(c => new WebSocketChannel
		{
			Id = c.Id,
			Name = c.Name,
			Created = c.Created,
			Color = c.Color,
			OwnerId = c.OwnerId,
			Password = null,
		}).ToListAsync();

		foreach (WebSocketChannel? channel in channels)
		{
			List<User> users = memoryStore.GetUsersInChannel(channel.Id ?? -1);
			channel.Users = users.Select(u => new WebSocketUser
			{
				Id = u.Id,
				Name = u.Name,
				Color = u.Color
			}).ToList();
		}

		var response = JsonSerializer.Serialize(new { command = "channels", channels }, options);
		System.Net.WebSockets.WebSocket? socket = memoryStore.GetSocketById(socketId);
		if (socket != null)
		{
			await webSocketSender.SendAsync(socket, response);
		}
	}

	public async Task CreateChannel(Channel channel, string socketId)
	{
		User? currentUser = memoryStore.GetUserBySocketId(socketId);

		if (currentUser == null)
		{
			await webSocketSender.SendErrorAsync(socketId, "Unauthorized");
			return;
		}

		(bool IsValid, string ErrorMessage) channelValidation = ChannelValidator.ValidateNewChannel(channel);

		Console.WriteLine(channelValidation.ErrorMessage);

		if (!channelValidation.IsValid)
		{
			await webSocketSender.SendErrorAsync(socketId, channelValidation.ErrorMessage);
			return;
		}

		channel.Owner = currentUser;
		channel.Created = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

		await dbContext.Channels.AddAsync(channel);
		await dbContext.SaveChangesAsync();

		var response = JsonSerializer.Serialize(new { command = "channelCreated", channel }, options);
		foreach (System.Net.WebSockets.WebSocket socket in memoryStore.GetAllSockets())
		{
			await webSocketSender.SendAsync(socket, response);
		}
	}
	public async Task UpdateChannel(Channel channel, string socketId)
	{
		Channel? currentChannel = await dbContext.Channels.FirstOrDefaultAsync(c => c.Id == channel.Id);
		User? owner = memoryStore.GetUserBySocketId(socketId);

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
		foreach (System.Net.WebSockets.WebSocket socket in memoryStore.GetAllSockets())
		{
			await webSocketSender.SendAsync(socket, response);
		}
	}

	public async Task DeleteChannel(int channelId, string socketId)
	{
		Channel? channel = await dbContext.Channels.FindAsync(channelId);
		User? owner = memoryStore.GetUserBySocketId(socketId);
		User? currentUser = memoryStore.GetUserBySocketId(socketId);

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

			System.Net.WebSockets.WebSocket? socket = memoryStore.GetSocketById(socketId);
			if (socket != null)
			{
				await webSocketSender.SendAsync(socket, response);
			}
		}
	}

	public async Task JoinChannel(int channelId, int userId)
	{
		Channel? channel = await dbContext.Channels.FirstOrDefaultAsync(c => c.Id == channelId);
		User? user = await dbContext.Users.FindAsync(userId);

		if (channel != null && user != null)
		{
			memoryStore.AddUserToChannel(channelId, user);
			List<User> users = memoryStore.GetUsersInChannel(channelId);
			var wsUsers = users.Select(u => u.ToDTO()).ToList();

			WebSocketChannel wsChannel = channel.ToDTO();
			wsChannel.Users = wsUsers;

			var response = JsonSerializer.Serialize(new { command = "userJoinedChannel", channel = wsChannel }, options);
			foreach (System.Net.WebSockets.WebSocket socket in memoryStore.GetAllSockets())
			{
				await webSocketSender.SendAsync(socket, response);
			}
		}
	}

	public async Task LeaveChannel(int channelId, int userId)
	{
		Channel? channel = await dbContext.Channels.FirstOrDefaultAsync(c => c.Id == channelId);
		User? user = await dbContext.Users.FindAsync(userId);

		if (channel != null && user != null)
		{
			memoryStore.RemoveUserFromChannel(channelId, user);
			List<User> users = memoryStore.GetUsersInChannel(channelId);
			var wsUsers = users.Select(u => u.ToDTO()).ToList();

			WebSocketChannel wsChannel = channel.ToDTO();
			wsChannel.Users = wsUsers;

			var response = JsonSerializer.Serialize(new { command = "userLeftChannel", channel = wsChannel }, options);
			foreach (System.Net.WebSockets.WebSocket socket in memoryStore.GetAllSockets())
			{
				await webSocketSender.SendAsync(socket, response);
			}
		}
	}
}
