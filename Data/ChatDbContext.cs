using B4mServer.Models;
using Microsoft.EntityFrameworkCore;

namespace B4mServer.Data;

public class ChatDbContext(DbContextOptions<ChatDbContext> options) : DbContext(options)
{
	public DbSet<User> Users { get; set; }
	public DbSet<Message> Messages { get; set; }
	public DbSet<Channel> Channels { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// Configure User entity
		modelBuilder.Entity<User>(entity =>
		{
			entity.ToTable("user");
			entity.Property(e => e.Id).HasColumnName("id");
			entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200);
			entity.Property(e => e.Password).HasColumnName("password").HasMaxLength(200);
			entity.Property(e => e.Color).HasColumnName("color");
			entity.Property(e => e.Joined).HasColumnName("joined");
		});

		// Configure Channel entity
		modelBuilder.Entity<Channel>(entity =>
		{
			entity.ToTable("channel");
			entity.Property(e => e.Id).HasColumnName("id");
			entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(50);
			entity.Property(e => e.Password).HasColumnType("password").HasMaxLength(50);
			entity.Property(e => e.OwnerId).HasColumnName("owner_id");
			entity.Property(e => e.Created).HasColumnName("created");

			// Configure the one-to-many relationship with User
			entity.HasMany(c => c.Users)
				  .WithOne()
				  .HasForeignKey("channel_id");
		});

		// Configure Message entity
		modelBuilder.Entity<Message>(entity =>
		{
			entity.ToTable("message");
			entity.Property(e => e.Id).HasColumnName("id");
			entity.Property(e => e.Text).HasColumnName("text").HasMaxLength(300);
			entity.Property(e => e.Time).HasColumnName("time");
			entity.Property(e => e.UserId).HasColumnName("user_id");
			entity.Property(e => e.ChannelId).HasColumnName("channel_id");

			// Configure the relationships
			entity.HasOne(m => m.User)
				  .WithMany()
				  .HasForeignKey(m => m.UserId);

			entity.HasOne(m => m.Channel)
				  .WithMany()
				  .HasForeignKey(m => m.ChannelId);
		});
	}
}
