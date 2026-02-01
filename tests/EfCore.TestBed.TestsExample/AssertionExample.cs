using EfCore.TestBed.Assertions;
using EfCore.TestBed.Core;
using EfCore.TestBed.Extensions;
using EfCore.TestBed.TestsExample.Entities;
using Microsoft.EntityFrameworkCore;

namespace EfCore.TestBed.TestsExample;

/// <summary>
/// Example showing assertion helpers.
/// </summary>
public class AssertionExample : EfTestBase<SampleDbContext>
{
  protected override void Seed(SampleDbContext context)
  {
    context.Users.AddRange(
        new User { Id = 1, Name = "John", Email = "john@example.com" },
        new User { Id = 2, Name = "Jane", Email = "jane@example.com" }
    );
  }

  [Fact]
  public void ShouldHave_FindsExistingEntity()
  {
    Db.ShouldHave<User>(u => u.Name == "John");
  }

  [Fact]
  public void ShouldNotHave_PassesWhenNotExists()
  {
    Db.ShouldNotHave<User>(u => u.Name == "NonExistent");
  }

  [Fact]
  public void ShouldHaveCount_MatchesExpected()
  {
    Db.ShouldHaveCount<User>(2);
  }

  [Fact]
  public void ShouldFailOnSave_CatchesFKViolation()
  {
    Db.Orders.Add(new Order { UserId = 999, Total = 100 });

    var ex = Db.ShouldFailOnSave<DbUpdateException>();
    Assert.NotNull(ex);
  }

  [Fact]
  public void DbAssert_StaticMethods_Work()
  {
    DbAssert.Exists<User>(Db, u => u.Id == 1);
    DbAssert.NotExists<User>(Db, u => u.Id == 999);
    DbAssert.Count<User>(Db, 2);
    DbAssert.NotEmpty<User>(Db);
  }
}
