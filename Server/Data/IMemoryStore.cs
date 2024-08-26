using B4mServer.Models;
using System.Net.WebSockets;

namespace B4mServer.Data;

public interface IMemoryStore
{
    void AddSocketConnection(string socketId, WebSocket socket);
    void RemoveSocketConnection(string socketId);
    int GetChannelIdByUserId(int userId);
    WebSocket? GetSocketById(string socketId);
    IEnumerable<WebSocket> GetAllSockets();
    void AddUser(string socketId, User user);
    void RemoveUser(string socketId);
    User? GetUserBySocketId(string socketId);
    User? GetUserById(int userId);
    void AddUserToChannel(int channelId, User user);
    List<User> GetUsersInChannel(int channelId);
    void RemoveUserFromChannel(int channelId, User user);
}