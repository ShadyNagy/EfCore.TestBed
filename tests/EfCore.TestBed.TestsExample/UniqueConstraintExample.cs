using EfCore.TestBed.Core;
using EfCore.TestBed.TestsExample.Entities;
using Microsoft.EntityFrameworkCore;

namespace EfCore.TestBed.TestsExample;

/// <summary>
/// Example showing unique constraint validation.
/// </summary>
public class UniqueConstraintExample : EfTestBase<SampleDbContext>
{
  protected override void Seed(SampleDbContext context)
  {
    context.Users.Add(new User { Name = "Existing", Email = "existing@example.com" });
  }

  [Fact]
  public void DuplicateEmail_ThrowsException()
  {
    Db.Users.Add(new User { Name = "New User", Email = "existing@example.com" }); // Duplicate!

    Assert.Throws<DbUpdateException>(() => Db.SaveChanges());
  }

  [Fact]
  public void UniqueEmail_Succeeds()
  {
    Db.Users.Add(new User { Name = "New User", Email = "new@example.com" }); // Unique
    Db.SaveChanges();

    Assert.Equal(2, Db.Users.Count());
  }
}
