using EfCore.TestBed.Core;
using EfCore.TestBed.TestsExample.Entities;

namespace EfCore.TestBed.TestsExample;

/// <summary>
/// Example showing cascade delete behavior.
/// </summary>
public class CascadeDeleteExample : EfTestBase<SampleDbContext>
{
  protected override void Seed(SampleDbContext context)
  {
    var user = new User { Id = 1, Name = "John", Email = "john@example.com" };
    context.Users.Add(user);
    context.SaveChanges();

    context.Orders.AddRange(
        new Order { UserId = 1, Total = 100 },
        new Order { UserId = 1, Total = 200 }
    );
  }

  [Fact]
  public void DeleteUser_CascadeDeletesOrders()
  {
    Assert.Equal(2, Db.Orders.Count());

    var user = Db.Users.Find(1)!;
    Db.Users.Remove(user);
    Db.SaveChanges();

    Assert.Equal(0, Db.Users.Count());
    Assert.Equal(0, Db.Orders.Count()); // Cascade deleted!
  }
}
