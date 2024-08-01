namespace B4mServer.Models;

public class Channel
{
	public int Id { get; set; }
	public required string Name { get; set; }

	// UNIX timestamp
	public long Created { get; set; }

	public string? Password { get; set; }

	public required string Color { get; set; }

	public List<User>? Users { get; set; }

	public int OwnerId { get; set; }
}
