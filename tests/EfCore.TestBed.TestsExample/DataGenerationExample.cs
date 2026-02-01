using EfCore.TestBed.Core;
using EfCore.TestBed.TestsExample.Entities;

namespace EfCore.TestBed.TestsExample;

/// <summary>
/// Example showing test data generation.
/// </summary>
public class DataGenerationExample : EfTestBase<SampleDbContext>
{
  [Fact]
  public void Generate_CreatesValidEntity()
  {
    var user = TestDataGenerator.Generate<User>();

    Assert.NotNull(user.Name);
    Assert.NotNull(user.Email);
    Assert.Contains("@", user.Email);
  }

  [Fact]
  public void Generate_WithOverrides_AppliesCustomValues()
  {
    var user = TestDataGenerator.Generate<User>(u =>
    {
      u.Name = "Custom Name";
    });

    Assert.Equal("Custom Name", user.Name);
    Assert.NotNull(user.Email); // Still generated
  }

  [Fact]
  public void GenerateMany_CreatesMultiple()
  {
    var users = TestDataGenerator.GenerateMany<User>(10, (u, i) =>
    {
      u.Email = $"user{i}@test.com"; // Ensure unique emails
    });

    Assert.Equal(10, users.Count);
    Assert.All(users, u => Assert.NotNull(u.Name));
  }

  [Fact]
  public void UtilityMethods_GenerateValidData()
  {
    var email = TestDataGenerator.GenerateEmail();
    var phone = TestDataGenerator.GeneratePhone();
    var name = TestDataGenerator.GenerateName();

    Assert.Contains("@", email);
    Assert.Contains("-", phone);
    Assert.Contains(" ", name);
  }
}
