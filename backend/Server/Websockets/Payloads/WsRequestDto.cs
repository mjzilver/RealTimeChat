using System.Text.Json;

namespace RealTimeChatServer.Websockets.Payloads;

public record WsRequestDto
{
	public int Version { get; init; } = 1;
	public required string Type { get; init; }
	public JsonElement? Payload { get; init; }
}