using EfCore.TestBed.Fixtures;
using EfCore.TestBed.TestsExample.Entities;

namespace EfCore.TestBed.TestsExample;

/// <summary>
/// Example shared fixture for multiple test classes.
/// </summary>
public class SampleDbFixture : SharedDbFixture<SampleDbContext>
{
  protected override void SeedData(SampleDbContext context)
  {
    context.Users.Add(new User { Name = "Shared User", Email = "shared@example.com" });
    context.Products.Add(new Product { Name = "Shared Product", SKU = "SP-001", Price = 50 });
  }
}
