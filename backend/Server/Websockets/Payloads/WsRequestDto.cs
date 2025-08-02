namespace RealTimeChatServer.Websockets.Payloads;

public record WsRequestDto
{
	public required string Command { get; init; }
	public WsMessageDto? Message { get; init; }
	public WsUserDto? User { get; init; }
	public WsChannelDto? Channel { get; init; }
}