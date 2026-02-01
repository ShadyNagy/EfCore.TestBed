using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using EfCore.TestBed.Configuration;

namespace EfCore.TestBed.Fixtures;

/// <summary>
/// Fixture that provides a shared database connection across multiple tests.
/// Use with xUnit's IClassFixture or ICollectionFixture.
/// </summary>
/// <typeparam name="TContext">The DbContext type</typeparam>
public class SharedDbFixture<TContext> : IDisposable
    where TContext : DbContext
{
  private readonly SqliteConnection _connection;
  private readonly DbContextOptions<TContext> _options;
  private bool _disposed;

  /// <summary>
  /// Gets the DbContext options for creating new contexts.
  /// </summary>
  public DbContextOptions<TContext> Options => _options;

  /// <summary>
  /// Creates a new shared database fixture.
  /// </summary>
  public SharedDbFixture() : this(null)
  {
  }

  /// <summary>
  /// Creates a new shared database fixture with custom configuration.
  /// </summary>
  public SharedDbFixture(Action<TestBedOptions>? configure)
  {
    var testBedOptions = new TestBedOptions();
    configure?.Invoke(testBedOptions);

    _connection = new SqliteConnection("Data Source=:memory:");
    _connection.Open();

    using var command = _connection.CreateCommand();
    command.CommandText = "PRAGMA foreign_keys = ON;";
    command.ExecuteNonQuery();

    var optionsBuilder = new DbContextOptionsBuilder<TContext>();
    optionsBuilder.UseSqlite(_connection);

    if (testBedOptions.EnableSensitiveDataLogging)
      optionsBuilder.EnableSensitiveDataLogging();

    if (testBedOptions.EnableDetailedErrors)
      optionsBuilder.EnableDetailedErrors();

    _options = optionsBuilder.Options;

    using var context = CreateContext();
    context.Database.EnsureCreated();

    SeedData(context);
    context.SaveChanges();
  }

  /// <summary>
  /// Override to seed initial data for the shared database.
  /// </summary>
  protected virtual void SeedData(TContext context)
  {
  }

  /// <summary>
  /// Creates a new DbContext instance using the shared connection.
  /// </summary>
  public TContext CreateContext()
  {
    var constructor = typeof(TContext).GetConstructor(new[] { typeof(DbContextOptions<TContext>) });
    if (constructor != null)
    {
      return (TContext)constructor.Invoke(new object[] { _options });
    }

    constructor = typeof(TContext).GetConstructor(new[] { typeof(DbContextOptions) });
    if (constructor != null)
    {
      return (TContext)constructor.Invoke(new object[] { _options });
    }

    throw new InvalidOperationException(
        $"DbContext type '{typeof(TContext).Name}' must have a constructor that accepts DbContextOptions.");
  }

  /// <summary>
  /// Clears all data from all tables (but keeps schema).
  /// </summary>
  public void ClearAllTables()
  {
    using var context = CreateContext();

    foreach (var entityType in context.Model.GetEntityTypes())
    {
      var tableName = entityType.GetTableName();
      if (tableName != null)
      {
        context.Database.ExecuteSqlRaw("DELETE FROM \"" + tableName + "\"");
      }
    }
  }

  /// <summary>
  /// Resets the database (drops and recreates schema).
  /// </summary>
  public void ResetDatabase()
  {
    using var context = CreateContext();
    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();
    SeedData(context);
    context.SaveChanges();
  }

  public void Dispose()
  {
    if (!_disposed)
    {
      _connection.Close();
      _connection.Dispose();
      _disposed = true;
    }
  }
}
