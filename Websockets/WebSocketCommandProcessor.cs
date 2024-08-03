using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using B4mServer.Models;

namespace B4mServer.Websockets
{
	public class WebSocketCommandProcessor
	{
		private readonly UserCommandProcessor _userCommandProcessor;
		private readonly ChannelCommandProcessor _channelCommandProcessor;
		private readonly MessageCommandProcessor _messageCommandProcessor;
		private readonly MemoryStore _memoryStore;
		private readonly WebSocketSender _webSocketSender;
		private readonly JsonSerializerOptions _options;

		public WebSocketCommandProcessor(UserCommandProcessor userCommandProcessor, 
			ChannelCommandProcessor channelCommandProcessor, 
			MessageCommandProcessor messageCommandProcessor,
			WebSocketSender webSocketSender,
			MemoryStore memoryStore,
			JsonSerializerOptions options)
		{
			_userCommandProcessor = userCommandProcessor;
			_channelCommandProcessor = channelCommandProcessor;
			_messageCommandProcessor = messageCommandProcessor;
			_memoryStore = memoryStore;
			_webSocketSender = webSocketSender;
			_options = options;
		}

		public async Task ProcessCommandAsync(WebSocketCommand command, string socketId)
		{
			Console.WriteLine($"Command: {command.Command}");	

			try
			{
				switch (command.Command)
				{
					case "broadcastMessage":
						await _messageCommandProcessor.BroadcastMessage(command.Message!.ToModel());
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
				await _webSocketSender.SendErrorAsync(socketId, ex.Message);
			}
		}

		internal async void UserDisconnected(string socketId)
		{
			await _userCommandProcessor.Logout(socketId);
		}
	}
}
