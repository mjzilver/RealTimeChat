using B4mServer.Models;
using Microsoft.EntityFrameworkCore;

namespace B4mServer.Data;

public class AppDbContext : DbContext
{
	public DbSet<User> Users { get; set; }
	public DbSet<Message> Messages { get; set; }
	public DbSet<Channel> Channels { get; set; }

	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		// User entity configuration
		modelBuilder.Entity<User>()
			.Property(u => u.Name)
			.HasMaxLength(200);

		modelBuilder.Entity<User>()
			.Property(u => u.Password)
			.HasMaxLength(200);

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
			.WithMany()
			.OnDelete(DeleteBehavior.Restrict);

		modelBuilder.Entity<Channel>()
			.Property(c => c.Name)
			.HasMaxLength(50);

		modelBuilder.Entity<Channel>()
			.Property(c => c.Password)
			.HasMaxLength(50);
	}
}
