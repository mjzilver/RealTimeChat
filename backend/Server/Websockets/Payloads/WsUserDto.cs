using RealTimeChatServer.Models;

namespace RealTimeChatServer.Websockets.Payloads;
public record WsUserDto
{
	public int? Id { get; init; }
	public required string Name { get; init; }
	public string? Password { get; init; }
	public required string Color { get; init; }
	public long Joined { get; init; }
	public bool ExistingUser { get; init; }

	public User ToModel() => new()
	{
		Id = Id ?? 0,
		Name = Name,
		Password = Password,
		Color = Color,
		Joined = Joined
	};
}
