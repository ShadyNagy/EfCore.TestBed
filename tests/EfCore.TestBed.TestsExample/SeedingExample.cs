using EfCore.TestBed.Core;
using EfCore.TestBed.Seeding;
using EfCore.TestBed.TestsExample.Entities;

namespace EfCore.TestBed.TestsExample;

/// <summary>
/// Example showing seeding extension methods.
/// </summary>
public class SeedingExample : EfTestBase<SampleDbContext>
{
  [Fact]
  public void SeedOne_AddsAndSaves()
  {
    var user = Db.SeedOne(new User { Name = "Seeded", Email = "seeded@example.com" });

    Assert.True(user.Id > 0);
    Assert.Equal(1, Db.Users.Count());
  }

  [Fact]
  public void SeedMany_WithFactory_CreatesMultiple()
  {
    var products = Db.SeedMany(5, i => new Product
    {
      Name = $"Product {i}",
      SKU = $"SKU-{i:D4}",
      Price = 10m * (i + 1),
      Stock = 100
    });

    Assert.Equal(5, products.Count);
    Assert.Equal(5, Db.Products.Count());
  }

  [Fact]
  public void FluentSeeding_BuilderPattern()
  {
    Db.Seed()
        .Add(new User { Name = "User 1", Email = "user1@example.com" })
        .Add(new User { Name = "User 2", Email = "user2@example.com" })
        .Add(3, i => new Product
        {
          Name = $"Product {i}",
          SKU = $"P-{i}",
          Price = i * 5,
          Stock = 10
        })
        .Build();

    Assert.Equal(2, Db.Users.Count());
    Assert.Equal(3, Db.Products.Count());
  }

  [Fact]
  public void SeedIfEmpty_OnlyWhenEmpty()
  {
    Db.SeedIfEmpty(new User { Name = "First", Email = "first@example.com" });
    Db.SeedIfEmpty(new User { Name = "Second", Email = "second@example.com" }); // Ignored!

    Assert.Equal(1, Db.Users.Count());
    Assert.Equal("First", Db.Users.First().Name);
  }
}
