using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using B4mServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace B4mServer.Websockets;

public class WebSocketHandler
{
	private readonly HttpContext _context;
	private readonly WebSocket _webSocket;
	private readonly AppDbContext _dbContext;
	private readonly MemoryStore _memoryStore;

	private readonly JsonSerializerOptions options = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		ReferenceHandler = ReferenceHandler.IgnoreCycles,
	};

	public WebSocketHandler(HttpContext context, WebSocket webSocket, AppDbContext dbContext, MemoryStore memoryStore)
	{
		_context = context;
		_webSocket = webSocket;
		_dbContext = dbContext;
		_memoryStore = memoryStore;

		Console.WriteLine("New WebSocket connection");
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
				var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
				await HandleMessage(socketId, message);
			}
			else if (result.MessageType == WebSocketMessageType.Close)
			{
				_memoryStore.RemoveSocketConnection(socketId);
				await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
			}
		}
	}

	private async Task HandleMessage(string socketId, string message)
	{
		Console.WriteLine($"Received message: {message}");

		var parsedMessage = JsonSerializer.Deserialize<WebSocketCommand>(message, options);

		if (parsedMessage == null) return;

		Console.WriteLine($"Received message: {parsedMessage.Command}");

		try
		{
			switch (parsedMessage.Command)
			{
				case "broadcast":
					await BroadcastMessage(parsedMessage.Message!.ToModel());
					break;
				case "getMessages":
					await GetMessages(parsedMessage.Channel!.Id ?? -1, socketId);
					break;
				case "getChannels":
					await GetChannels(socketId);
					break;
				case "joinChannel":
					await JoinChannel(parsedMessage.Channel!.Id ?? -1, parsedMessage.User!.Id ?? -1);
					break;
				case "leaveChannel":
					await LeaveChannel(parsedMessage.Channel!.Id ?? -1, parsedMessage.User!.Id ?? -1);
					break;
				case "createChannel":
					await CreateChannel(parsedMessage.Channel!.ToModel(), socketId);
					break;
				case "updateChannel":
					await UpdateChannel(parsedMessage.Channel!.ToModel(), socketId);
					break;
				case "deleteChannel":
					await DeleteChannel(parsedMessage.Channel!.Id ?? -1, socketId);
					break;
				case "getUsers":
					await GetUsers(socketId);
					break;
				case "loginUser":
					await Login(parsedMessage.User!.ToModel(), socketId);
					break;
				case "logoutUser":
					await Logout(parsedMessage.Channel!.Id ?? -1, socketId);
					break;
				case "registerUser":
					await Register(parsedMessage.User!.ToModel(), socketId);
					break;
				case "updateUser":
					await UpdateUser(parsedMessage.User!.ToModel(), socketId);
					break;
				default:
					Console.WriteLine($"Unknown command: {parsedMessage.Command}");
					break;
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error: {ex.Message}");
			await SendErrorAsync(socketId, ex.Message);
		}
	}

	private async Task SendAsync(WebSocket socket, string message)
	{
		if (socket.State == WebSocketState.Open)
		{
			var encodedMessage = Encoding.UTF8.GetBytes(message);
			await socket.SendAsync(new ArraySegment<byte>(encodedMessage), WebSocketMessageType.Text, true, CancellationToken.None);
		}
	}

	private async Task BroadcastMessage(Message message)
	{
		Console.WriteLine(JsonSerializer.Serialize(message, options));

		message.User = await _dbContext.Users.FindAsync(message.UserId) ?? new User();
		message.Channel = await _dbContext.Channels.FindAsync(message.ChannelId) ?? new Channel();

		if (message.User == null || message.Channel == null)
		{
			return;
		}

		foreach (var socket in _memoryStore.GetAllSockets())
		{
			await SendAsync(socket, JsonSerializer.Serialize(new { command = "broadcast", message }, options));
		}

		await _dbContext.Messages.AddAsync(message);
		await _dbContext.SaveChangesAsync();
	}

	private async Task GetMessages(int channelId, string socketId)
	{
		var messages = await _dbContext.Messages.Where(m => m.ChannelId == channelId).Include(m => m.User).ToListAsync();
		var response = JsonSerializer.Serialize(new { command = "messages", channelId, messages }, options);
		var socket = _memoryStore.GetSocketById(socketId);

		if (socket != null)
		{
			await SendAsync(socket, response);
		}
	}

	private async Task GetChannels(string socketId)
	{
		var channels = await _dbContext.Channels.ToListAsync();
		var response = JsonSerializer.Serialize(new { command = "channels", channels }, options);
		var socket = _memoryStore.GetSocketById(socketId);
		if (socket != null)
		{
			await SendAsync(socket, response);
		}
	}

	private async Task JoinChannel(int channelId, int userId)
	{
		var channel = await _dbContext.Channels.FirstOrDefaultAsync(c => c.Id == channelId);
		var user = await _dbContext.Users.FindAsync(userId);

		if (channel != null && user != null)
		{
			Console.WriteLine($"Joining channel {channel.Name} with user {user.Name}");

			_memoryStore.AddUserToChannel(channelId, user);
			var users = _memoryStore.GetUsersInChannel(channelId);
			// serialze users
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

			var response = JsonSerializer.Serialize(new { command = "userJoinedChannel", channel = wsChannel }, options);
			foreach (var socket in _memoryStore.GetAllSockets())
			{
				await SendAsync(socket, response);
			}
		}
	}

	private async Task LeaveChannel(int channelId, int userId)
	{
		Console.WriteLine($"Leaving channel {channelId} with user {userId}");
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

			var response = JsonSerializer.Serialize(new { command = "userLeftChannel", channel = wsChannel }, options);
			foreach (var socket in _memoryStore.GetAllSockets())
			{
				await SendAsync(socket, response);
			}
		} else
		{
			Console.WriteLine("Failed to leave channel");
		}
	}

	private async Task CreateChannel(Channel channel, string socketId)
	{
		var currentUser = _memoryStore.GetUserBySocketId(socketId);

		if (currentUser == null)
		{
			await SendErrorAsync(socketId, "Unauthorized");
			return;
		}

		channel.Owner = currentUser;

		await _dbContext.Channels.AddAsync(channel);
		await _dbContext.SaveChangesAsync();

		var response = JsonSerializer.Serialize(new { command = "channelCreated", channel }, options);
		foreach (var socket in _memoryStore.GetAllSockets())
		{
			await SendAsync(socket, response);
		}
	}

	private async Task UpdateChannel(Channel channel, string socketId)
	{
		var currentChannel = _dbContext.Channels.FirstOrDefault(c => c.Id == channel.Id);
		var owner = _memoryStore.GetUserBySocketId(socketId);

		if(currentChannel == null)
		{
			await SendErrorAsync(socketId, "Channel does not exit");
			return;
		}

		if (owner == null || owner.Id != currentChannel.Owner!.Id) {
			await SendErrorAsync(socketId, "You do not own this channel");
			return;
		}

		currentChannel.Name = channel.Name;
		currentChannel.Color  = channel.Color;
		currentChannel.Password = channel.Password;
		currentChannel.Owner = owner;
		currentChannel.OwnerId = channel.OwnerId;

		await _dbContext.SaveChangesAsync();

		var response = JsonSerializer.Serialize(new { command = "channelUpdated", channel = currentChannel }, options);
		foreach (var socket in _memoryStore.GetAllSockets())
		{
			await SendAsync(socket, response);
		}
	}

	private async Task DeleteChannel(int channelId, string socketId)
	{
		var channel = await _dbContext.Channels.FindAsync(channelId);
		if (channel != null)
		{
			_dbContext.Channels.Remove(channel);
			await _dbContext.SaveChangesAsync();

			var response = JsonSerializer.Serialize(new { command = "channelDeleted", channel }, options);
			foreach (var socket in _memoryStore.GetAllSockets())
			{
				await SendAsync(socket, response);
			}
		}
		else
		{
			await SendErrorAsync(socketId, "Failed to delete channel");
		}
	}

	private async Task GetUsers(string socketId)
	{
		var users = await _dbContext.Users.ToListAsync();
		var response = JsonSerializer.Serialize(new { command = "users", users }, options);
		var socket = _memoryStore.GetSocketById(socketId);
		if (socket != null)
		{
			await SendAsync(socket, response);
		}
	}

	private async Task Login(User user, string socketId)
	{
		var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Name == user.Name && u.Password == user.Password);
		if (existingUser != null)
		{
			_memoryStore.AddUser(socketId, existingUser);

			var response = JsonSerializer.Serialize(new { command = "login", user = existingUser }, options);
			await SendAsync(_memoryStore.GetSocketById(socketId), response);
		}
		else
		{
			await SendErrorAsync(socketId, "Invalid login credentials");
		}
	}

	private async Task Logout(int channelId, string socketId)
	{
		var user = _memoryStore.GetUserBySocketId(socketId);
		if (user != null)
		{
			var channel = await _dbContext.Channels.FirstOrDefaultAsync(c => c.Id == channelId);
			if (channel != null)
			{
				_memoryStore.RemoveUserFromChannel(channelId, user);
				await _dbContext.SaveChangesAsync();

				var response = JsonSerializer.Serialize(new { command = "logout", user, channel }, options);
				_memoryStore.RemoveUser(socketId);
				foreach (var socket in _memoryStore.GetAllSockets())
				{
					await SendAsync(socket, response);
				}
			}
		}
	}

	private async Task Register(User user, string socketId)
	{
		Console.WriteLine($"Registering user: {user.Name}");

		var existingUser = await _dbContext.Users.AnyAsync(u => u.Name == user.Name);
		if (!existingUser)
		{
			await _dbContext.Users.AddAsync(user);
			await _dbContext.SaveChangesAsync();

			var response = JsonSerializer.Serialize(new { command = "register", user }, options);
			_memoryStore.AddUser(socketId, user);
			await SendAsync(_memoryStore.GetSocketById(socketId), response);
		}
		else
		{
			await SendErrorAsync(socketId, "User already exists");
		}
	}

	private async Task UpdateUser(User user, string socketId)
	{
		Console.WriteLine(user.Id);
		var existingUser = await _dbContext.Users.FindAsync(user.Id);
		if (existingUser == null)
		{
			await SendErrorAsync(socketId, "User not found");
			return;
		}

		if (_memoryStore.GetUserBySocketId(socketId)?.Id != user.Id)
		{
			await SendErrorAsync(socketId, "Unauthorized");
			return;
		}

		existingUser.Color = user.Color;

		await _dbContext.SaveChangesAsync();

		var response = JsonSerializer.Serialize(new { command = "userUpdated", user = existingUser }, options);
		foreach (var socket in _memoryStore.GetAllSockets())
		{
			await SendAsync(socket, response);
		}
	}

	private async Task SendErrorAsync(string socketId, string message)
	{
		var response = JsonSerializer.Serialize(new { command = "error", error = message }, options);
		var socket = _memoryStore.GetSocketById(socketId);
		if (socket != null)
		{
			await SendAsync(socket, response);
		}
	}
}
