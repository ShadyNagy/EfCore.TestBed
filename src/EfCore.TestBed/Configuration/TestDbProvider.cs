namespace EfCore.TestBed.Configuration;

/// <summary>
/// Supported database providers.
/// </summary>
public enum TestDbProvider
{
  /// <summary>
  /// SQLite in-memory database (recommended). Provides real database behavior.
  /// </summary>
  SqliteInMemory,

  /// <summary>
  /// SQLite file-based database. Persists between tests.
  /// </summary>
  SqliteFile,

  /// <summary>
  /// EF Core InMemory provider. Fast but no constraint validation.
  /// </summary>
  InMemory
}
