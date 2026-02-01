using EfCore.TestBed.Core;
using EfCore.TestBed.TestsExample.Entities;
using EfCore.TestBed.Transactions;

namespace EfCore.TestBed.TestsExample;

/// <summary>
/// Example showing transaction helpers.
/// </summary>
public class TransactionExample : EfTestBase<SampleDbContext>
{
  protected override void Seed(SampleDbContext context)
  {
    context.Users.Add(new User { Id = 1, Name = "Original", Email = "original@example.com" });
  }

  [Fact]
  public void InRollbackTransaction_AutomaticallyRollsBack()
  {
    var initialCount = Db.Users.Count();

    Db.InRollbackTransaction(ctx =>
    {
      ctx.Users.Add(new User { Name = "Temp", Email = "temp@example.com" });
      ctx.SaveChanges();
      // Would be 2 here
    });

    // After rollback, back to 1
    Assert.Equal(initialCount, Db.Users.Count());
  }

  [Fact]
  public void RollbackScope_ManualControl()
  {
    using var scope = Db.CreateRollbackScope();

    Db.Users.Add(new User { Name = "Scoped", Email = "scoped@example.com" });
    Db.SaveChanges();

    Assert.Equal(2, Db.Users.Count()); // Exists within scope

    scope.Rollback();

    // After explicit rollback, back to original
    ClearChangeTracker();
    Assert.Equal(1, Db.Users.Count());
  }

  [Fact]
  public void InTransaction_CommitsOnSuccess()
  {
    Db.InTransaction(ctx =>
    {
      ctx.Users.Add(new User { Name = "Committed", Email = "committed@example.com" });
      ctx.SaveChanges();
    });

    Assert.Equal(2, Db.Users.Count()); // Committed!
  }

  [Fact]
  public void InTransaction_RollsBackOnException()
  {
    try
    {
      Db.InTransaction(ctx =>
      {
        ctx.Users.Add(new User { Name = "WillRollback", Email = "rollback@example.com" });
        ctx.SaveChanges();
        throw new Exception("Simulated failure");
      });
    }
    catch { /* Expected */ }

    ClearChangeTracker();
    Assert.Equal(1, Db.Users.Count()); // Rolled back!
  }
}
