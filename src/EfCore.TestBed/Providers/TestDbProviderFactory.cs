using EfCore.TestBed.Configuration;

namespace EfCore.TestBed.Providers;

/// <summary>
/// Factory for creating test database providers.
/// </summary>
public static class TestDbProviderFactory
{
  /// <summary>
  /// Creates a provider based on the specified type.
  /// </summary>
  public static ITestDbProvider Create(TestDbProvider providerType, TestBedOptions? options = null)
  {
    return providerType switch
    {
      TestDbProvider.SqliteInMemory => new SqliteInMemoryProvider(),
      TestDbProvider.SqliteFile => new SqliteFileProvider(options?.ConnectionString),
      TestDbProvider.InMemory => new InMemoryProvider(options?.DatabaseName),
      _ => throw new ArgumentOutOfRangeException(nameof(providerType))
    };
  }
}
