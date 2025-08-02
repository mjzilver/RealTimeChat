using RealTimeChatServer.Websockets.Payloads;

namespace RealTimeChatServer.Models;

public partial class Message
{
	public int Id { get; set; }
	public int UserId { get; set; }
	public int ChannelId { get; set; }
	public string Text { get; set; } = null!;
	public long Time { get; set; }

	// Navigational Properties
	public virtual User User { get; set; } = null!;
	public virtual Channel Channel { get; set; } = null!;

	public WsMessageDto ToDTO()
	{
		return new WsMessageDto
        {
			Id = Id,
			Text = Text,
			Time = Time,
			UserId = UserId,
			ChannelId = ChannelId,
			User = User?.ToDTO(),
			Channel = Channel?.ToDTO()
		};
	}

	public WsMessageDto ToMinimalDTO()
	{
		return new WsMessageDto
        {
			Id = Id,
			Text = Text,
			Time = Time,
			UserId = UserId,
			ChannelId = ChannelId
		};
	}
}
