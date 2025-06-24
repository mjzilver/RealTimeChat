using RealTimeChatServer.Websockets.Interfaces;

namespace RealTimeChatServer.Websockets;

public class WebSocketCommandProcessor(
	IUserCommandProcessor userCommandProcessor,
	IChannelCommandProcessor channelCommandProcessor,
	IMessageCommandProcessor messageCommandProcessor)
	: IWebSocketCommandProcessor
{
	private readonly IUserCommandProcessor _userCommandProcessor = userCommandProcessor;
	private readonly IChannelCommandProcessor _channelCommandProcessor = channelCommandProcessor;
	private readonly IMessageCommandProcessor _messageCommandProcessor = messageCommandProcessor;

	public async Task ProcessCommandAsync(WebSocketCommand command, string socketId)
	{
		Console.WriteLine($"Command: {command.Command}");

		try
		{
			switch (command.Command)
			{
				case "broadcastMessage":
					await _messageCommandProcessor.BroadcastMessage(command.Message!.ToModel(), socketId);
					break;
				case "getMessages":
					await _messageCommandProcessor.GetMessages(command.Channel?.Id ?? 0, socketId);
					break;
				case "getChannels":
					await _channelCommandProcessor.GetChannels(socketId);
					break;
				case "createChannel":
					await _channelCommandProcessor.CreateChannel(command.Channel!.ToModel(), socketId);
					break;
				case "deleteChannel":
					await _channelCommandProcessor.DeleteChannel(command.Channel!.Id ?? -1, socketId);
					break;
				case "updateChannel":
					await _channelCommandProcessor.UpdateChannel(command.Channel!.ToModel(), socketId);
					break;
				case "joinChannel":
					await _channelCommandProcessor.JoinChannel(command.Channel!.Id ?? -1, command.User!.Id ?? -1);
					break;
				case "leaveChannel":
					await _channelCommandProcessor.LeaveChannel(command.Channel!.Id ?? -1, command.User!.Id ?? -1);
					break;
				case "getUsers":
					await _userCommandProcessor.GetUsers(socketId);
					break;
				case "loginUser":
					await _userCommandProcessor.Login(command.User!.ToModel(), socketId);
					break;
				case "logoutUser":
					await _userCommandProcessor.Logout(socketId);
					break;
				case "registerUser":
					await _userCommandProcessor.Register(command.User!.ToModel(), socketId);
					break;
				case "updateUser":
					await _userCommandProcessor.UpdateUser(command.User!.ToModel(), socketId);
					break;
				default:
					Console.WriteLine($"Unknown command: {command.Command}");
					break;
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error: {ex.Message}");
		}
	}

	public async void UserDisconnected(string socketId)
	{
		await _userCommandProcessor.Logout(socketId);
	}
}
