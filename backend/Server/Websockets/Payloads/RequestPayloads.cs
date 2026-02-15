namespace RealTimeChatServer.Websockets.Payloads;

public record RegisterUserPayload(
	string Username,
	string Password,
	string? Color);

public record LoginUserPayload(
	string Username,
	string Password);

public record BroadcastMessagePayload(
	int UserId,
	int ChannelId,
	string Text);

public record CreateChannelPayload(
	string Name,
	string? Password,
	string Color,
	int? OwnerId);

public record UpdateChannelPayload(
	int Id,
	string Name,
	string? Password,
	string Color,
	int? OwnerId);

public record JoinLeavePayload(
	int ChannelId,
	int UserId);

public record GetMessagesPayload(
	int ChannelId);

public record UpdateUserPayload(
	int Id,
	string Name,
	string Color);

public record DeleteChannelPayload(
	int Id);