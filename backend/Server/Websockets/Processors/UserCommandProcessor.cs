using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using RealTimeChatServer.Data;
using RealTimeChatServer.Models;
using RealTimeChatServer.Validators;
using RealTimeChatServer.Websockets.Interfaces;

namespace RealTimeChatServer.Websockets.Processors;

public class UserCommandProcessor(
	AppDbContext dbContext,
	IMemoryStore memoryStore,
	IWebSocketSender webSocketSender,
	JsonSerializerOptions options
) : IUserCommandProcessor
{
	public string HashPassword(string password)
	{
		return BCrypt.Net.BCrypt.HashPassword(password);
	}

	public bool VerifyPassword(string password, string hash)
	{
		return BCrypt.Net.BCrypt.Verify(password, hash);
	}

	public async Task GetUsers(string socketId)
	{
		var users = await dbContext.Users
			.Select(u => new Payloads.UserPayload(u.Id, u.Name, u.Color, u.Joined, true))
			.ToListAsync();
		var envelope = new { type = "users", payload = new { users } };
		var response = JsonSerializer.Serialize(envelope, options);
		var socket = memoryStore.GetSocketById(socketId);
		if (socket != null)
		{
			await webSocketSender.SendAsync(socket, response);
		}
	}

	public async Task Login(User user, string socketId)
	{
		var existingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Name == user.Name);


		if (existingUser != null)
		{
			if (!VerifyPassword(user.Password!, existingUser.Password!))
			{
				await webSocketSender.SendErrorAsync(socketId, "Invalid login credentials");
				return;
			}

			memoryStore.AddUser(socketId, existingUser);

			var userPayload = new Payloads.UserPayload(existingUser.Id, existingUser.Name, existingUser.Color, existingUser.Joined);
			var response = JsonSerializer.Serialize(new { type = "loginUser", payload = new { user = userPayload } }, options);
			var socket = memoryStore.GetSocketById(socketId);
			if (socket != null)
				await webSocketSender.SendAsync(socket, response);
		}
		else
		{
			await webSocketSender.SendErrorAsync(socketId, "Invalid login credentials");
		}
	}

	public async Task Logout(string socketId)
	{
		var user = memoryStore.GetUserBySocketId(socketId);
		if (user != null)
		{
			var channelId = memoryStore.GetChannelIdByUserId(user.Id);

			var channel = await dbContext.Channels.FirstOrDefaultAsync(c => c.Id == channelId);
			if (channel != null && channelId.HasValue)
			{
				memoryStore.RemoveUserFromChannel(channelId.Value, user);
				await dbContext.SaveChangesAsync();

				var userPayload = new Payloads.UserPayload(user.Id, user.Name, user.Color, user.Joined);
				var response = JsonSerializer.Serialize(new { type = "logoutUser", payload = new { user = userPayload, channel } }, options);
				memoryStore.RemoveUser(socketId);
				foreach (var socket in memoryStore.GetAllSockets())
				{
					await webSocketSender.SendAsync(socket, response);
				}
			}
		}
	}

	public async Task Register(User user, string socketId)
	{
		var existingUser = await dbContext.Users.AnyAsync(u => u.Name == user.Name);
		if (!existingUser)
		{
			(var IsValid, var ErrorMessage) = UserValidator.ValidateNewUser(user);
			if (!IsValid)
			{
				await webSocketSender.SendErrorAsync(socketId, ErrorMessage);
				return;
			}

			user.Password = HashPassword(user.Password!);
			user.Joined = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

			await dbContext.Users.AddAsync(user);
			await dbContext.SaveChangesAsync();

			var userPayload = new Payloads.UserPayload(user.Id, user.Name, user.Color, user.Joined);
			var response = JsonSerializer.Serialize(new { type = "registerUser", payload = new { user = userPayload } }, options);
			memoryStore.AddUser(socketId, user);
			var socket = memoryStore.GetSocketById(socketId);

			if (socket == null) return;

			await webSocketSender.SendAsync(socket, response);
		}
		else
		{
			await webSocketSender.SendErrorAsync(socketId, "User already exists");
		}
	}


	public async Task UpdateUser(User user, string socketId)
	{
		var existingUser = await dbContext.Users.FindAsync(user.Id);
		if (existingUser == null)
		{
			await webSocketSender.SendErrorAsync(socketId, "User not found");
			return;
		}

		if (memoryStore.GetUserBySocketId(socketId)?.Id != user.Id)
		{
			await webSocketSender.SendErrorAsync(socketId, "Unauthorized");
			return;
		}

		(var IsValid, var ErrorMessage) = UserValidator.ValidateUpdatedUser(user, existingUser);
		if (!IsValid)
		{
			await webSocketSender.SendErrorAsync(socketId, ErrorMessage);
			return;
		}

		existingUser.Color = user.Color;

		await dbContext.SaveChangesAsync();

		var userPayload = new Payloads.UserPayload(existingUser.Id, existingUser.Name, existingUser.Color, existingUser.Joined);
		var response = JsonSerializer.Serialize(new { type = "userUpdated", payload = new { user = userPayload } }, options);
		foreach (var socket in memoryStore.GetAllSockets())
		{
			await webSocketSender.SendAsync(socket, response);
		}
	}
}
