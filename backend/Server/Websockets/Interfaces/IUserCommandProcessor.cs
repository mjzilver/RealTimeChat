using RealTimeChatServer.Models;

namespace RealTimeChatServer.Websockets.Interfaces;

public interface IUserCommandProcessor
{
	Task Login(User user, string socketId);
	Task Logout(string socketId);
	Task Register(User user, string socketId);
	Task UpdateUser(User user, string socketId);
	Task GetUsers(string socketId);
}
