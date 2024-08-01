namespace B4mServer.Models;

public class User
{
    public required int Id { get; set; }
	public required string Name { get; set; }
	public string? Password { get; set; }
	// UNIX timestamp
	public required long Joined { get; set; }
	public required string Color { get; set; }
}
