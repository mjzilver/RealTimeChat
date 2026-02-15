namespace RealTimeChatServer.Websockets.Payloads;

public record UserPayload(
	int Id,
	string Name,
	string Color,
	long Joined,
	bool ExistingUser = true);

public record MessagePayload(
	int Id,
	int UserId,
	int ChannelId,
	string Text,
	long Time,
	UserPayload? User = null);

public record ChannelPayload(
	int Id,
	string Name,
	string Color,
	long Created,
	string? Password = null,
	int? OwnerId = null,
	List<UserPayload>? Users = null,
	List<MessagePayload>? Messages = null);
