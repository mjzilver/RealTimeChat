using RealTimeChatServer.Models;

namespace RealTimeChatServer.Websockets.Payloads;

public record WsChannelDto
{
	public int? Id { get; init; }
	public required string Name { get; init; }
	public long Created { get; init; }
	public required string Color { get; init; }
	public string? Password { get; init; }
	public int? OwnerId { get; init; }
	public int? UserId { get; init; }
	public int? ChannelId { get; init; }
	public ICollection<WsUserDto>? Users { get; init; } = [];
	public ICollection<WsMessageDto>? Messages { get; init; } = [];

	public Channel ToModel() => new()
	{
		Id = Id ?? 0,
		Name = Name,
		Created = Created,
		Color = Color,
		Password = Password,
		OwnerId = OwnerId
	};
}