using EfCore.TestBed.Benchmarks.Entities;
using Microsoft.EntityFrameworkCore;

namespace EfCore.TestBed.Benchmarks;

public class BenchmarkDbContext : DbContext
{
  public DbSet<User> Users => Set<User>();
  public DbSet<Order> Orders => Set<Order>();
  public DbSet<OrderItem> OrderItems => Set<OrderItem>();
  public DbSet<Product> Products => Set<Product>();

  public BenchmarkDbContext(DbContextOptions<BenchmarkDbContext> options) : base(options) { }

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
