using System;
using System.Collections.Generic;
using B4mServer.Models;
using Microsoft.EntityFrameworkCore;

namespace B4mServer.Data;

public partial class ForumContext : DbContext
{
	public ForumContext(DbContextOptions<ForumContext> options) : base(options)
	{
	}

	public virtual DbSet<Channel> Channels { get; set; }
	public virtual DbSet<Message> Messages { get; set; }
	public virtual DbSet<User> Users { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Channel>(entity =>
		{
			entity.ToTable("channel");

			entity.HasIndex(e => e.Name, "IX_channel_name").IsUnique();

			entity.Property(e => e.Id).HasColumnName("id");
			entity.Property(e => e.Color).HasColumnName("color");
			entity.Property(e => e.Created).HasColumnName("created");
			entity.Property(e => e.Name).HasColumnName("name");
			entity.Property(e => e.OwnerId).HasColumnName("owner_id");
			entity.Property(e => e.Password).HasColumnName("password");

			// Relationships
			entity.HasMany(e => e.Messages)
				.WithOne(m => m.Channel)
				.HasForeignKey(m => m.ChannelId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasOne(e => e.Owner)
				.WithMany(u => u.OwnedChannels)
				.HasForeignKey(e => e.OwnerId)
				.OnDelete(DeleteBehavior.Restrict);
		});

		modelBuilder.Entity<Message>(entity =>
		{
			entity.ToTable("message");

			entity.Property(e => e.Id).HasColumnName("id");
			entity.Property(e => e.ChannelId).HasColumnName("channel_id");
			entity.Property(e => e.Text).HasColumnName("text");
			entity.Property(e => e.Time).HasColumnName("time");
			entity.Property(e => e.UserId).HasColumnName("user_id");

			// Relationships
			entity.HasOne(m => m.Channel)
				.WithMany(c => c.Messages)
				.HasForeignKey(m => m.ChannelId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasOne(m => m.User)
				.WithMany(u => u.Messages)
				.HasForeignKey(m => m.UserId)
				.OnDelete(DeleteBehavior.Restrict);
		});

		modelBuilder.Entity<User>(entity =>
		{
			entity.ToTable("user");

			entity.HasIndex(e => e.Name, "IX_user_name").IsUnique();

			entity.Property(e => e.Id).HasColumnName("id");
			entity.Property(e => e.Color).HasColumnName("color");
			entity.Property(e => e.Joined).HasColumnName("joined");
			entity.Property(e => e.Name).HasColumnName("name");
			entity.Property(e => e.Password).HasColumnName("password");

			// Relationships
			entity.HasMany(e => e.Messages)
				.WithOne(m => m.User)
				.HasForeignKey(m => m.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			entity.HasMany(e => e.OwnedChannels)
				.WithOne(c => c.Owner)
				.HasForeignKey(c => c.OwnerId)
				.OnDelete(DeleteBehavior.Restrict);
		});
	}
}
