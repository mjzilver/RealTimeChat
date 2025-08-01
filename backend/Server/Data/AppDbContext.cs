﻿using Microsoft.EntityFrameworkCore;

using RealTimeChatServer.Models;

namespace RealTimeChatServer.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
	public DbSet<User> Users { get; set; }
	public DbSet<Message> Messages { get; set; }
	public DbSet<Channel> Channels { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		// User entity configuration
		modelBuilder.Entity<User>()
			.Property(u => u.Name)
			.HasMaxLength(200);

		modelBuilder.Entity<User>()
			.Property(u => u.Password)
			.HasMaxLength(200);

		modelBuilder.Entity<User>()
			.HasMany(u => u.OwnedChannels)
			.WithOne(c => c.Owner)
			.HasForeignKey(c => c.OwnerId)
			.OnDelete(DeleteBehavior.Restrict);

		// Message entity configuration
		modelBuilder.Entity<Message>()
			.HasOne(m => m.User)
			.WithMany(u => u.Messages)
			.HasForeignKey(m => m.UserId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<Message>()
			.HasOne(m => m.Channel)
			.WithMany(c => c.Messages)
			.HasForeignKey(m => m.ChannelId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<Message>()
			.Property(m => m.Text)
			.HasMaxLength(300);

		// Channel entity configuration
		modelBuilder.Entity<Channel>()
			.HasOne(c => c.Owner)
			.WithMany(u => u.OwnedChannels)
			.HasForeignKey(c => c.OwnerId)
			.OnDelete(DeleteBehavior.Restrict);

		modelBuilder.Entity<Channel>()
			.Property(c => c.Name)
			.HasMaxLength(50);

		modelBuilder.Entity<Channel>()
			.Property(c => c.Password)
			.HasMaxLength(50);
	}
}
