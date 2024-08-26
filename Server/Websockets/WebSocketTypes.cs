using B4mServer.Models;

namespace B4mServer.Websockets;

public record WebSocketCommand
{
	public required string Command { get; set; }
	public WebSocketMessage? Message { get; set; }
	public WebSocketUser? User { get; set; }
	public WebSocketChannel? Channel { get; set; }
}

public record WebSocketMessage
{
	public int? Id { get; set; }
	public required string Text { get; set; }
	public long Time { get; set; }
	public int UserId { get; set; }
	public int ChannelId { get; set; }

	public WebSocketUser? User { get; set; } 
	public WebSocketChannel? Channel { get; set; }

	public Message ToModel()
	{
		return new Message
		{
			Id = Id ?? 0,
			Text = Text,
			Time = Time,
			UserId = User?.Id ?? -1,
			ChannelId = Channel?.Id ?? -1
		};
	}
}

public record WebSocketUser
{
	public int? Id { get; set; }
	public required string Name { get; set; }
	public string? Password { get; set; }
	public required string Color { get; set; }
	public long Joined { get; set; }
	public bool ExistingUser { get; set; }

	public User ToModel()
	{
		return new User
		{
			Id = Id ?? 0,
			Name = Name,
			Password = Password,
			Color = Color,
			Joined = Joined
		};
	}
}

public record WebSocketChannel
{
	public int? Id { get; set; }
	public required string Name { get; set; }
	public long Created { get; set; }
	public required string Color { get; set; }
	public string? Password { get; set; }
	public int? OwnerId { get; set; }
	public int? UserId { get; set; }
	public int? ChannelId { get; set; }
	public ICollection<WebSocketUser>? Users { get; set; } = [];
	public ICollection<WebSocketMessage>? Messages { get; set; } = [];

	public Channel ToModel()
	{
		return new Channel
		{
			Id = Id ?? 0,
			Name = Name,
			Created = Created,
			Color = Color,
			Password = Password,
			OwnerId = OwnerId
		};
	}
}
