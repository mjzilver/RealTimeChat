using System.Text.Json.Serialization;

namespace B4mServer.Models;

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
}
