using B4mServer.Websockets;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace B4mServer.Models;

public partial class Channel
{
	public int Id { get; set; }
	public string Name { get; set; } = null!;
	public long Created { get; set; }
	public string Color { get; set; } = null!;
	[JsonIgnore]
	public string? Password { get; set; }

	// foreign key
	public int OwnerId { get; set; }

	// Navigational Properties
	[JsonIgnore]
	[ForeignKey("OwnerId")]
	public virtual User? Owner { get; set; }
	[JsonIgnore]
	public virtual ICollection<Message> Messages { get; set; } = [];

	public WebSocketChannel ToDTO()
	{
		return new WebSocketChannel
		{
			Id = Id,
			Name = Name,
			Created = Created,
			Color = Color,
			OwnerId = OwnerId,
			Messages = Messages.Select(m => m.ToMinimalDTO()).ToList()
		};
	}
}
