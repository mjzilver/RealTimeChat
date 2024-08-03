using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

	public Channel GetSerialized() 	{
		return new Channel
		{
			Id = Id,
			Name = Name,
			Created = Created,
			Color = Color,
			OwnerId = OwnerId
		};
	}
}
