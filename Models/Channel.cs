using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace B4mServer.Models;

public partial class Channel
{
    public int Id { get; set; }

    public int? OwnerId { get; set; }

    public string Name { get; set; } = null!;

    public decimal Created { get; set; }

    public string Color { get; set; } = null!;

    public string? Password { get; set; }

	// Navigational Properties
	[JsonIgnore]
	public virtual User? Owner { get; set; }

    [JsonIgnore]
    public virtual ICollection<Message> Messages { get; set; } = [];
}
