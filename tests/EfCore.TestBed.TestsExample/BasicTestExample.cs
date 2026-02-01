using EfCore.TestBed.Core;
using EfCore.TestBed.TestsExample.Entities;
using Microsoft.EntityFrameworkCore;

namespace EfCore.TestBed.TestsExample;

/// <summary>
/// Example showing basic EfTestBase usage with seeding.
/// </summary>
public class BasicTestExample : EfTestBase<SampleDbContext>
{
  protected override void Seed(SampleDbContext context)
  {
    context.Users.Add(new User
    {
      Id = 1,
      Name = "John Doe",
      Email = "john@example.com"
    });

    context.Products.AddRange(
        new Product { Id = 1, Name = "Widget", SKU = "WDG-001", Price = 9.99m, Stock = 100 },
        new Product { Id = 2, Name = "Gadget", SKU = "GDG-001", Price = 19.99m, Stock = 50 }
    );
  }

  [Fact]
  public void User_Exists_AfterSeeding()
  {
    var user = Db.Users.Find(1);

    Assert.NotNull(user);
    Assert.Equal("John Doe", user.Name);
  }

  [Fact]
  public void CreateOrder_WithValidUser_Succeeds()
  {
    var order = new Order
    {
      UserId = 1,
      Total = 29.98m,
      Status = "Pending"
    };

    Db.Orders.Add(order);
    Db.SaveChanges();

    Assert.True(order.Id > 0);
    Assert.Equal(1, Db.Orders.Count());
  }

  [Fact]
  public void CreateOrder_WithInvalidUser_ThrowsException()
  {
    var order = new Order
    {
      UserId = 999, // User doesn't exist!
      Total = 50m
    };

    Db.Orders.Add(order);

    Assert.Throws<DbUpdateException>(() => Db.SaveChanges());
  }
}
