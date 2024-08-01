using System.ComponentModel.DataAnnotations;

namespace B4mServer.Models;

public class Message
{
	[Key]
	public int Id { get; set; }
	public string Text { get; set; } = "";
	public int UserId { get; set; }
	public int ChannelId { get; set; }
	
	// UNIX timestamp
	public long Time { get; set; }

	// Navigation properties
	public User? User { get; set; }
	public Channel? Channel { get; set; }
}