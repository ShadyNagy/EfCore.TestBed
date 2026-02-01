using EfCore.TestBed.Core;
using EfCore.TestBed.TestsExample.Entities;
using Microsoft.EntityFrameworkCore;

namespace EfCore.TestBed.TestsExample;

/// <summary>
/// Example showing restrict delete behavior.
/// </summary>
public class RestrictDeleteExample : EfTestBase<SampleDbContext>
{
  protected override void Seed(SampleDbContext context)
  {
    context.Users.Add(new User { Id = 1, Name = "John", Email = "john@example.com" });
    context.Products.Add(new Product { Id = 1, Name = "Widget", SKU = "W-001", Price = 10 });
    context.SaveChanges();

    context.Orders.Add(new Order { Id = 1, UserId = 1, Total = 10 });
    context.SaveChanges();

    context.OrderItems.Add(new OrderItem
    {
      OrderId = 1,
      ProductId = 1,
      Quantity = 1,
      UnitPrice = 10
    });
  }

  [Fact]
  public void DeleteProduct_WithOrderItems_Fails()
  {
    ClearChangeTracker();

    var product = Db.Products.Find(1)!;
    Db.Products.Remove(product);

    Assert.Throws<DbUpdateException>(() => Db.SaveChanges());
  }
}
