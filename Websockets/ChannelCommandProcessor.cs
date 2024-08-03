using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using B4mServer.Models;
using B4mServer.Data;

namespace B4mServer.Websockets;

public class ChannelCommandProcessor
{
	private readonly AppDbContext _dbContext;
	private readonly MemoryStore _memoryStore;
	private readonly JsonSerializerOptions _options;
	private readonly WebSocketSender _websocketSender;
	public ChannelCommandProcessor(AppDbContext dbContext, MemoryStore memoryStore,
		WebSocketSender webSocketSender, JsonSerializerOptions options)
	{
		_dbContext = dbContext;
		_memoryStore = memoryStore;
		_options = options;
		_websocketSender = webSocketSender;
	}

	public async Task GetChannels(string socketId)
	{
		var channels = await _dbContext.Channels.Select(c => new WebSocketChannel
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
			var users = _memoryStore.GetUsersInChannel(channel.Id ?? -1);
			channel.Users = users.Select(u => new WebSocketUser
			{
				Id = u.Id,
				Name = u.Name,
				Color = u.Color
			}).ToList();
		}

		var response = JsonSerializer.Serialize(new { command = "channels", channels }, _options);
		var socket = _memoryStore.GetSocketById(socketId);
		if (socket != null)
		{
			await _websocketSender.SendAsync(socket, response);
		}
	}

	public async Task CreateChannel(Channel channel, string socketId)
	{
		var currentUser = _memoryStore.GetUserBySocketId(socketId);

		if (currentUser == null)
		{
			await _websocketSender.SendErrorAsync(socketId, "Unauthorized");
			return;
		}

		channel.Owner = currentUser;

		await _dbContext.Channels.AddAsync(channel);
		await _dbContext.SaveChangesAsync();

		var response = JsonSerializer.Serialize(new { command = "channelCreated", channel }, _options);
		foreach (var socket in _memoryStore.GetAllSockets())
		{
			await _websocketSender.SendAsync(socket, response);
		}
	}
	public async Task UpdateChannel(Channel channel, string socketId)
	{
		var currentChannel = _dbContext.Channels.FirstOrDefault(c => c.Id == channel.Id);
		var owner = _memoryStore.GetUserBySocketId(socketId);

		if (currentChannel == null)
		{
			await _websocketSender.SendErrorAsync(socketId, "Channel does not exit");
			return;
		}

		if (owner == null || owner.Id != currentChannel.Owner!.Id)
		{
			await _websocketSender.SendErrorAsync(socketId, "You do not own this channel");
			return;
		}

		currentChannel.Name = channel.Name;
		currentChannel.Color = channel.Color;
		currentChannel.Password = channel.Password;
		currentChannel.Owner = owner;
		currentChannel.OwnerId = channel.OwnerId;

		await _dbContext.SaveChangesAsync();

		var response = JsonSerializer.Serialize(new { command = "channelUpdated", channel = currentChannel }, _options);
		foreach (var socket in _memoryStore.GetAllSockets())
		{
			await _websocketSender.SendAsync(socket, response);
		}
	}

	public async Task DeleteChannel(int channelId, string socketId)
	{
		var channel = await _dbContext.Channels.FindAsync(channelId);
		if (channel != null)
		{
			_dbContext.Channels.Remove(channel);
			await _dbContext.SaveChangesAsync();

			var response = JsonSerializer.Serialize(new { command = "deleteChannel", channelId }, _options);
			var socket = _memoryStore.GetSocketById(socketId);
			if (socket != null)
			{
				await _websocketSender.SendAsync(socket, response);
			}
		}
	}

	public async Task JoinChannel(int channelId, int userId)
	{
		var channel = await _dbContext.Channels.FirstOrDefaultAsync(c => c.Id == channelId);
		var user = await _dbContext.Users.FindAsync(userId);

		if (channel != null && user != null)
		{
			_memoryStore.AddUserToChannel(channelId, user);
			var users = _memoryStore.GetUsersInChannel(channelId);
			// serialze users
			var wsUsers = users.Select(u => u.ToDTO()).ToList();

			WebSocketChannel wsChannel = channel.ToDTO();
			wsChannel.Users = wsUsers;

			var response = JsonSerializer.Serialize(new { command = "userJoinedChannel", channel = wsChannel }, _options);
			foreach (var socket in _memoryStore.GetAllSockets())
			{
				await _websocketSender.SendAsync(socket, response);
			}
		}
	}

	public async Task LeaveChannel(int channelId, int userId)
	{
		var channel = await _dbContext.Channels.FirstOrDefaultAsync(c => c.Id == channelId);
		var user = await _dbContext.Users.FindAsync(userId);

		if (channel != null && user != null)
		{
			_memoryStore.RemoveUserFromChannel(channelId, user);
			var users = _memoryStore.GetUsersInChannel(channelId);
			var wsUsers = users.Select(u => new WebSocketUser
			{
				Id = u.Id,
				Name = u.Name,
				Color = u.Color
			}).ToList();

			WebSocketChannel wsChannel = new()
			{
				Id = channel.Id,
				Name = channel.Name,
				Created = channel.Created,
				Color = channel.Color,
				Password = null, // don't send password to client
				OwnerId = channel.OwnerId,
				Users = wsUsers
			};

			var response = JsonSerializer.Serialize(new { command = "userLeftChannel", channel = wsChannel }, _options);
			foreach (var socket in _memoryStore.GetAllSockets())
			{
				await _websocketSender.SendAsync(socket, response);
			}
		}
	}
}
