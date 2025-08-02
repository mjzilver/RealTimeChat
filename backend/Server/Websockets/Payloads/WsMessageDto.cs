using RealTimeChatServer.Models;

namespace RealTimeChatServer.Websockets.Payloads;

public record WsMessageDto
{
	public int? Id { get; init; }
	public required string Text { get; init; }
	public long Time { get; init; }
	public int UserId { get; init; }
	public int ChannelId { get; init; }

	public WsUserDto? User { get; init; }
	public WsChannelDto? Channel { get; init; }

	public Message ToModel() => new()
	{
		Id = Id ?? 0,
		Text = Text,
		Time = Time,
		UserId = User?.Id ?? -1,
		ChannelId = Channel?.Id ?? -1
	};
}