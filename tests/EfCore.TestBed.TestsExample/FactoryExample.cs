using EfCore.TestBed.Factory;
using EfCore.TestBed.TestsExample.Entities;

namespace EfCore.TestBed.TestsExample;

/// <summary>
/// Example showing TestDb factory usage.
/// </summary>
public class FactoryExample
{
  [Fact]
  public void Create_WithSeed_WorksCorrectly()
  {
    using var testDb = TestDb.Create<SampleDbContext>(ctx =>
    {
      ctx.Users.Add(new User { Name = "Test User", Email = "test@example.com" });
    });

    Assert.Equal(1, testDb.Context.Users.Count());
  }

  [Fact]
  public void Quick_CreatesEmptyDatabase()
  {
    using var db = TestDb.Quick<SampleDbContext>();

    Assert.Equal(0, db.Users.Count());

    db.Users.Add(new User { Name = "New User", Email = "new@example.com" });
    db.SaveChanges();

    Assert.Equal(1, db.Users.Count());
  }

  [Fact]
  public async Task CreateAsync_WithAsyncSeed_Works()
  {
    using var testDb = await TestDb.CreateAsync<SampleDbContext>(async ctx =>
    {
      ctx.Users.Add(new User { Name = "Async User", Email = "async@example.com" });
      await Task.CompletedTask; // Simulate async operation
    });

    Assert.Equal(1, testDb.Context.Users.Count());
  }
}
