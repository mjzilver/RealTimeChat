using RealTimeChatServer.Websockets.Interfaces;

namespace RealTimeChatServer.Websockets;

public class WebSocketCommandProcessor(
	IUserCommandProcessor userCommandProcessor,
	IChannelCommandProcessor channelCommandProcessor,
	IMessageCommandProcessor messageCommandProcessor
) : IWebSocketCommandProcessor
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
					if (command.Message is null)
						throw new ArgumentNullException(nameof(command.Message));
					await _messageCommandProcessor.BroadcastMessage(command.Message.ToModel(), socketId);
					break;

				case "getMessages":
					if (command.Channel?.Id is not int channelId)
						throw new ArgumentNullException("Channel ID is required");
					await _messageCommandProcessor.GetMessages(channelId, socketId);
					break;

				case "getChannels":
					await _channelCommandProcessor.GetChannels(socketId);
					break;

				case "createChannel":
					if (command.Channel is null)
						throw new ArgumentNullException(nameof(command.Channel));
					await _channelCommandProcessor.CreateChannel(command.Channel.ToModel(), socketId);
					break;

				case "deleteChannel":
					if (command.Channel?.Id is not int deleteId)
						throw new ArgumentNullException("Channel ID is required for deletion");
					await _channelCommandProcessor.DeleteChannel(deleteId, socketId);
					break;

				case "updateChannel":
					if (command.Channel is null)
						throw new ArgumentNullException(nameof(command.Channel));
					await _channelCommandProcessor.UpdateChannel(command.Channel.ToModel(), socketId);
					break;

				case "joinChannel":
					if (command.Channel?.Id is not int joinChannelId || command.User?.Id is not int joinUserId)
						throw new ArgumentException("Channel ID and User ID are required for joining");
					await _channelCommandProcessor.JoinChannel(joinChannelId, joinUserId);
					break;

				case "leaveChannel":
					if (command.Channel?.Id is not int leaveChannelId || command.User?.Id is not int leaveUserId)
						throw new ArgumentException("Channel ID and User ID are required for leaving");
					await _channelCommandProcessor.LeaveChannel(leaveChannelId, leaveUserId);
					break;

				case "getUsers":
					await _userCommandProcessor.GetUsers(socketId);
					break;

				case "loginUser":
					if (command.User is null)
						throw new ArgumentNullException(nameof(command.User));
					await _userCommandProcessor.Login(command.User.ToModel(), socketId);
					break;

				case "logoutUser":
					await _userCommandProcessor.Logout(socketId);
					break;

				case "registerUser":
					if (command.User is null)
						throw new ArgumentNullException(nameof(command.User));
					await _userCommandProcessor.Register(command.User.ToModel(), socketId);
					break;

				case "updateUser":
					if (command.User is null)
						throw new ArgumentNullException(nameof(command.User));
					await _userCommandProcessor.UpdateUser(command.User.ToModel(), socketId);
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
