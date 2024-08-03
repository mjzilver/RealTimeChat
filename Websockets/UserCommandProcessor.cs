using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using B4mServer.Models;
using B4mServer.Data;

namespace B4mServer.Websockets;

public class UserCommandProcessor
{
	private readonly AppDbContext _dbContext;
	private readonly MemoryStore _memoryStore;
	private readonly JsonSerializerOptions _options;
	private readonly WebSocketSender _webSocketSender;

	public UserCommandProcessor(AppDbContext dbContext, MemoryStore memoryStore,
		WebSocketSender webSocketSender, JsonSerializerOptions options)
	{
		_dbContext = dbContext;
		_memoryStore = memoryStore;
		_options = options;
		_webSocketSender = webSocketSender;
	}

	public async Task GetUsers(string socketId)
	{
		var users = await _dbContext.Users.ToListAsync();
		var response = JsonSerializer.Serialize(new { command = "users", users }, _options);
		var socket = _memoryStore.GetSocketById(socketId);
		if (socket != null)
		{
			await _webSocketSender.SendAsync(socket, response);
		}
	}

	public async Task Login(User user, string socketId)
	{
		var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Name == user.Name && u.Password == user.Password);
		if (existingUser != null)
		{
			_memoryStore.AddUser(socketId, existingUser);

			var response = JsonSerializer.Serialize(new { command = "loginUser", user = existingUser }, _options);
			var socket = _memoryStore.GetSocketById(socketId);
			if(socket != null)
				await _webSocketSender.SendAsync(socket, response);
		}
		else
		{
			await _webSocketSender.SendErrorAsync(socketId, "Invalid login credentials");
		}
	}

	public async Task Logout(string socketId)
	{
		var user = _memoryStore.GetUserBySocketId(socketId);
		if (user != null)
		{
			var channelId = _memoryStore.GetChannelIdByUserId(user.Id);

			var channel = await _dbContext.Channels.FirstOrDefaultAsync(c => c.Id == channelId);
			if (channel != null)
			{
				_memoryStore.RemoveUserFromChannel(channelId, user);
				await _dbContext.SaveChangesAsync();

				var response = JsonSerializer.Serialize(new { command = "logoutUser", user, channel }, _options);
				_memoryStore.RemoveUser(socketId);
				foreach (var socket in _memoryStore.GetAllSockets())
				{
					await _webSocketSender.SendAsync(socket, response);
				}
			}
		}
	}

	public async Task Register(User user, string socketId)
	{
		var existingUser = await _dbContext.Users.AnyAsync(u => u.Name == user.Name);
		if (!existingUser)
		{
			await _dbContext.Users.AddAsync(user);
			await _dbContext.SaveChangesAsync();

			var response = JsonSerializer.Serialize(new { command = "registerUser", user }, _options);
			_memoryStore.AddUser(socketId, user);
			var socket = _memoryStore.GetSocketById(socketId);

			if (socket == null) return;

			await _webSocketSender.SendAsync(socket, response);
		}
		else
		{
			await _webSocketSender.SendErrorAsync(socketId, "User already exists");
		}
	}


	public async Task UpdateUser(User user, string socketId)
	{
		var existingUser = await _dbContext.Users.FindAsync(user.Id);
		if (existingUser == null)
		{
			await _webSocketSender.SendErrorAsync(socketId, "User not found");
			return;
		}

		if (_memoryStore.GetUserBySocketId(socketId)?.Id != user.Id)
		{
			await _webSocketSender.SendErrorAsync(socketId, "Unauthorized");
			return;
		}

		existingUser.Color = user.Color;

		await _dbContext.SaveChangesAsync();

		var response = JsonSerializer.Serialize(new { command = "userUpdated", user = existingUser }, _options);
		foreach (var socket in _memoryStore.GetAllSockets())
		{
			await _webSocketSender.SendAsync(socket, response);
		}
	}
}
