using System.Collections.Concurrent;
using System.Net.WebSockets;

using RealTimeChatServer.Models;

namespace RealTimeChatServer.Data;

public class MemoryStore : IMemoryStore
{
	private readonly ConcurrentDictionary<string, WebSocket> _connectedSockets = new();
	private readonly ConcurrentDictionary<string, User> _connectedUsers = new();
	private readonly ConcurrentDictionary<int, List<User>> _channelUsers = new();

	public void AddSocketConnection(string socketId, WebSocket socket)
	{
		_connectedSockets[socketId] = socket;
	}

	public void RemoveSocketConnection(string socketId)
	{
		_connectedSockets.TryRemove(socketId, out _);
	}

	public int GetChannelIdByUserId(int userId)
	{
		return _channelUsers.FirstOrDefault(c => c.Value.Exists(u => u.Id == userId)).Key;
	}

	public WebSocket? GetSocketById(string socketId)
	{
		_connectedSockets.TryGetValue(socketId, out var socket);
		return socket;
	}

	public IEnumerable<WebSocket> GetAllSockets()
	{
		return _connectedSockets.Values;
	}

	public void AddUser(string socketId, User user)
	{
		_connectedUsers[socketId] = user;
	}

	public void RemoveUser(string socketId)
	{
		_connectedUsers.TryRemove(socketId, out _);
	}

	public User? GetUserBySocketId(string socketId)
	{
		_connectedUsers.TryGetValue(socketId, out var user);
		return user;
	}

	public User? GetUserById(int userId)
	{
		return _connectedUsers.Values.FirstOrDefault(u => u.Id == userId);
	}

	public void AddUserToChannel(int channelId, User user)
	{
		if (_channelUsers.TryGetValue(channelId, out var users))
		{
			if (!users.Contains(user))
			{
				users.Add(user);
			}
		}
		else
		{
			_channelUsers[channelId] = [user];
		}
	}

	public void RemoveUserFromChannel(int channelId, User user)
	{
		if (_channelUsers.TryGetValue(channelId, out var users))
		{
			users.Remove(user);
		}
	}

	public void RemoveUserBySocketId(string socketId)
	{
		var user = GetUserBySocketId(socketId);
		if (user != null)
		{
			RemoveUserFromChannel(GetChannelIdByUserId(user.Id), user);
			RemoveUser(socketId);
		}
	}

	public List<User> GetUsersInChannel(int channelId)
	{
		_channelUsers.TryGetValue(channelId, out var users);
		return users ?? [];
	}
}
