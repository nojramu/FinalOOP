using Microsoft.EntityFrameworkCore;
using test.Models;

namespace test.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            
            // Ignore the computed Address property - it's not a database column
            entity.Ignore(e => e.Address);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.Id);
            // Ignore the computed DisplayName property - it's not a database column
            entity.Ignore(e => e.DisplayName);
            // Ensure DateTime properties are stored as TEXT in SQLite
            entity.Property(e => e.CreatedAt).HasColumnType("TEXT");
            entity.Property(e => e.AdminReplyDate).HasColumnType("TEXT");
        });
    }
}

