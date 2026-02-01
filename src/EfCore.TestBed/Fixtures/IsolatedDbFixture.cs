using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EfCore.TestBed.Fixtures;

/// <summary>
/// Fixture that provides an isolated database for each test class.
/// The database is created when the fixture is created and destroyed when disposed.
/// </summary>
/// <typeparam name="TContext">The DbContext type</typeparam>
public class IsolatedDbFixture<TContext> : IDisposable
  where TContext : DbContext
{
  private readonly SqliteConnection _connection;
  private readonly DbContextOptions<TContext> _options;
  private readonly Func<TContext, Task>? _seedAsync;
  private bool _disposed;
  private bool _initialized;

  public DbContextOptions<TContext> Options => _options;

  public IsolatedDbFixture(Func<TContext, Task>? seedAsync = null)
  {
    _seedAsync = seedAsync;
    _connection = new SqliteConnection("Data Source=:memory:");
    _connection.Open();

    using var command = _connection.CreateCommand();
    command.CommandText = "PRAGMA foreign_keys = ON;";
    command.ExecuteNonQuery();

    var optionsBuilder = new DbContextOptionsBuilder<TContext>()
      .UseSqlite(_connection)
      .EnableDetailedErrors();

    _options = optionsBuilder.Options;
  }

  /// <summary>
  /// Initializes the database (call once before tests).
  /// </summary>
  public async Task InitializeAsync()
  {
    if (_initialized) return;

    using var context = CreateContext();
    await context.Database.EnsureCreatedAsync();

    if (_seedAsync != null)
    {
      await _seedAsync(context);
      await context.SaveChangesAsync();
    }

    _initialized = true;
  }

  public TContext CreateContext()
  {
    var constructor = typeof(TContext).GetConstructor(new[] { typeof(DbContextOptions<TContext>) })
                      ?? typeof(TContext).GetConstructor(new[] { typeof(DbContextOptions) });

    if (constructor == null)
    {
      throw new InvalidOperationException(
        $"DbContext type '{typeof(TContext).Name}' must have a constructor that accepts DbContextOptions.");
    }

    return (TContext)constructor.Invoke(new object[] { _options });
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
