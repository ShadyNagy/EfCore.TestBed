using EfCore.TestBed.TestsExample.Entities;
using Microsoft.EntityFrameworkCore;

namespace EfCore.TestBed.TestsExample;

public class SampleDbContext : DbContext
{
  public DbSet<User> Users => Set<User>();
  public DbSet<Order> Orders => Set<Order>();
  public DbSet<OrderItem> OrderItems => Set<OrderItem>();
  public DbSet<Product> Products => Set<Product>();

  public SampleDbContext(DbContextOptions<SampleDbContext> options) : base(options) { }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<User>(e =>
    {
      e.HasIndex(u => u.Email).IsUnique();
    });

    modelBuilder.Entity<Order>(e =>
    {
      e.HasOne(o => o.User)
              .WithMany(u => u.Orders)
              .HasForeignKey(o => o.UserId)
              .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<OrderItem>(e =>
    {
      e.HasOne(oi => oi.Order)
              .WithMany(o => o.Items)
              .HasForeignKey(oi => oi.OrderId)
              .OnDelete(DeleteBehavior.Cascade);

      e.HasOne(oi => oi.Product)
              .WithMany()
              .HasForeignKey(oi => oi.ProductId)
              .OnDelete(DeleteBehavior.Restrict);
    });
  }
}
