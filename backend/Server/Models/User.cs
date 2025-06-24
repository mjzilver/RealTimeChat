using System.Text.Json.Serialization;

using RealTimeChatServer.Websockets;

namespace RealTimeChatServer.Models;
public partial class User
{
	public int Id { get; set; }
	public string Name { get; set; } = null!;
	[JsonIgnore]
	public string? Password { get; set; }
	public long Joined { get; set; }
	public string Color { get; set; } = null!;

	// Navigational Properties
	[JsonIgnore]
	public virtual ICollection<Message> Messages { get; set; } = [];
	[JsonIgnore]
	public virtual ICollection<Channel> OwnedChannels { get; set; } = [];

	public WebSocketUser ToDTO()
	{
		return new WebSocketUser
		{
			Id = Id,
			Name = Name,
			Password = null, // Password is not sent to the client
			Color = Color,
			Joined = Joined,
			ExistingUser = true
		};
	}
}