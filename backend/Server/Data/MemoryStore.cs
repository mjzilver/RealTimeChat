using System.Collections.Concurrent;
using System.Net.WebSockets;

using RealTimeChatServer.Models;

namespace RealTimeChatServer.Data;

public class MemoryStore : IMemoryStore
{
	private readonly ConcurrentDictionary<string, WebSocket> _connectedSockets = new();
	private readonly ConcurrentDictionary<string, User> _connectedUsers = new();
	private readonly ConcurrentDictionary<int, ConcurrentDictionary<int, User>> _channelUsers = new();
	private readonly ConcurrentDictionary<int, string> _userSocketMap = new();

	public void AddSocketConnection(string socketId, WebSocket socket)
	{
		_connectedSockets[socketId] = socket;
	}

	public void RemoveSocketConnection(string socketId)
	{
		_connectedSockets.TryRemove(socketId, out _);
	}

	public int? GetChannelIdByUserId(int userId)
	{
		foreach (var kv in _channelUsers)
		{
			if (kv.Value != null && kv.Value.ContainsKey(userId))
			{
				return kv.Key;
			}
		}
		return null;
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
		_userSocketMap[user.Id] = socketId;
	}

	public void RemoveUser(string socketId)
	{
		if (_connectedUsers.TryRemove(socketId, out var user))
		{
			_userSocketMap.TryRemove(user.Id, out _);
		}
	}

	public User? GetUserBySocketId(string socketId)
	{
		_connectedUsers.TryGetValue(socketId, out var user);
		return user;
	}

	public User? GetUserById(int userId)
	{
		if (_userSocketMap.TryGetValue(userId, out var socketId))
		{
			_connectedUsers.TryGetValue(socketId, out var user);
			return user;
		}
		return _connectedUsers.Values.FirstOrDefault(u => u.Id == userId);
	}

	public void AddUserToChannel(int channelId, User user)
	{
		var users = _channelUsers.GetOrAdd(channelId, _ => new ConcurrentDictionary<int, User>());
		users[user.Id] = user;
	}

	public void RemoveUserFromChannel(int channelId, User user)
	{
		if (_channelUsers.TryGetValue(channelId, out var users))
		{
			users.TryRemove(user.Id, out _);
			if (users.IsEmpty)
			{
				_channelUsers.TryRemove(channelId, out _);
			}
		}
	}

	public void RemoveUserBySocketId(string socketId)
	{
		var user = GetUserBySocketId(socketId);
		if (user != null)
		{
			var channelId = GetChannelIdByUserId(user.Id);
			if (channelId.HasValue)
			{
				RemoveUserFromChannel(channelId.Value, user);
			}
			RemoveUser(socketId);
		}
	}

	public IEnumerable<WebSocket> GetSocketsInChannel(int channelId)
	{
		if (!_channelUsers.TryGetValue(channelId, out var users)
			|| users == null
			|| users.IsEmpty)
		{
			return [];
		}

		var sockets = new List<WebSocket>();
		foreach (var kv in users)
		{
			var userId = kv.Key;
			if (_userSocketMap.TryGetValue(userId, out var socketId))
			{
				if (_connectedSockets.TryGetValue(socketId, out var socket) && socket.State == WebSocketState.Open)
				{
					sockets.Add(socket);
				}
			}
		}

		return sockets;
	}

	public List<User> GetUsersInChannel(int channelId)
	{
		if (_channelUsers.TryGetValue(channelId, out var users))
		{
			return [.. users.Values];
		}
		return [];
	}
}
