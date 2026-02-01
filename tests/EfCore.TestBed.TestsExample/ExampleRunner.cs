namespace EfCore.TestBed.TestsExample;

/// <summary>
/// Example using shared fixture (xUnit IClassFixture).
/// </summary>
// public class SharedFixtureExample : IClassFixture<SampleDbFixture>
// {
//     private readonly SampleDbContext _db;
//
//     public SharedFixtureExample(SampleDbFixture fixture)
//     {
//         _db = fixture.CreateContext();
//     }
//
//     [Fact]
//     public void SharedData_IsAvailable()
//     {
//         Assert.True(_db.Users.Any(u => u.Name == "Shared User"));
//         Assert.True(_db.Products.Any(p => p.Name == "Shared Product"));
//     }
// }

// ============================================
// RUN ALL EXAMPLES
// ============================================

public static class ExampleRunner
{
  public static void RunAll()
  {
    Console.WriteLine("Running EfCore.TestBed Examples...\n");

    // Basic tests
    RunExample("Basic Tests", () =>
    {
      using var test = new BasicTestExample();
      test.GetType().GetMethod("User_Exists_AfterSeeding")?.Invoke(test, null);
      test.GetType().GetMethod("CreateOrder_WithValidUser_Succeeds")?.Invoke(test, null);
    });

    // Factory tests
    RunExample("Factory Usage", () =>
    {
      var test = new FactoryExample();
      test.GetType().GetMethod("Create_WithSeed_WorksCorrectly")?.Invoke(test, null);
      test.GetType().GetMethod("Quick_CreatesEmptyDatabase")?.Invoke(test, null);
    });

    // Assertion tests
    RunExample("Assertions", () =>
    {
      using var test = new AssertionExample();
      test.GetType().GetMethod("ShouldHave_FindsExistingEntity")?.Invoke(test, null);
      test.GetType().GetMethod("ShouldHaveCount_MatchesExpected")?.Invoke(test, null);
    });

    // Seeding tests
    RunExample("Seeding", () =>
    {
      using var test = new SeedingExample();
      test.GetType().GetMethod("SeedOne_AddsAndSaves")?.Invoke(test, null);
      test.GetType().GetMethod("FluentSeeding_BuilderPattern")?.Invoke(test, null);
    });

    // Data generation tests
    RunExample("Data Generation", () =>
    {
      using var test = new DataGenerationExample();
      test.GetType().GetMethod("Generate_CreatesValidEntity")?.Invoke(test, null);
      test.GetType().GetMethod("UtilityMethods_GenerateValidData")?.Invoke(test, null);
    });

    // Transaction tests
    RunExample("Transactions", () =>
    {
      using var test = new TransactionExample();
      test.GetType().GetMethod("InRollbackTransaction_AutomaticallyRollsBack")?.Invoke(test, null);
    });

    // Cascade delete tests
    RunExample("Cascade Delete", () =>
    {
      using var test = new CascadeDeleteExample();
      test.GetType().GetMethod("DeleteUser_CascadeDeletesOrders")?.Invoke(test, null);
    });

    // Unique constraint tests
    RunExample("Unique Constraints", () =>
    {
      using var test = new UniqueConstraintExample();
      test.GetType().GetMethod("UniqueEmail_Succeeds")?.Invoke(test, null);
    });

    Console.WriteLine("\n✅ All examples completed successfully!");
  }

  private static void RunExample(string name, Action action)
  {
    try
    {
      action();
      Console.WriteLine($"  ✅ {name}");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"  ❌ {name}: {ex.Message}");
    }
  }
}
