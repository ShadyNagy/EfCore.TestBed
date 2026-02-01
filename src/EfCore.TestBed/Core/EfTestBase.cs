using Microsoft.EntityFrameworkCore;
using EfCore.TestBed.Configuration;
using EfCore.TestBed.Providers;

namespace EfCore.TestBed.Core;

/// <summary>
/// Base class for EF Core tests. Provides a fresh database for each test.
/// Works with xUnit, NUnit, and MSTest.
/// </summary>
/// <typeparam name="TContext">The DbContext type</typeparam>
public abstract class EfTestBase<TContext> : IDisposable, IAsyncDisposable
        where TContext : DbContext
{
  private ITestDbProvider? _provider;
  private TContext? _context;
  private bool _initialized;
  private bool _disposed;

  /// <summary>
  /// The DbContext instance for testing.
  /// </summary>
  protected TContext Db
  {
    get
    {
      EnsureInitialized();
      return _context!;
    }
  }

  /// <summary>
  /// Gets whether the provider supports real transactions.
  /// </summary>
  protected bool SupportsTransactions
  {
    get
    {
      EnsureInitialized();
      return _provider!.SupportsTransactions;
    }
  }

  /// <summary>
  /// Creates a new test instance with a fresh database.
  /// </summary>
  protected EfTestBase()
  {
    // Lazy initialization - context created on first access
  }

  /// <summary>
  /// Ensures the database is initialized.
  /// </summary>
  private void EnsureInitialized()
  {
    if (_initialized) return;

    var options = new TestBedOptions();
    ConfigureOptions(options);

    _provider = TestDbProviderFactory.Create(options.Provider, options);

    var optionsBuilder = new DbContextOptionsBuilder<TContext>();
    _provider.Configure(optionsBuilder);

    if (options.EnableSensitiveDataLogging)
      optionsBuilder.EnableSensitiveDataLogging();

    if (options.EnableDetailedErrors)
      optionsBuilder.EnableDetailedErrors();

    _context = CreateContext(optionsBuilder.Options);

    if (options.AutoCreateDatabase)
    {
      _context.Database.EnsureCreated();
    }

    Seed(_context);
    if (_context.ChangeTracker.HasChanges())
    {
      _context.SaveChanges();
    }

    _initialized = true;
  }

  /// <summary>
  /// Override this method to configure test options.
  /// Called before database initialization.
  /// </summary>
  /// <example>
  /// protected override void ConfigureOptions(TestBedOptions options)
  /// {
  ///     options.EnableSensitiveDataLogging = true;
  ///     options.Provider = TestDbProvider.InMemory;
  /// }
  /// </example>
  protected virtual void ConfigureOptions(TestBedOptions options)
  {
    // Default: no custom configuration
  }

  /// <summary>
  /// Override this method to seed test data.
  /// SaveChanges is called automatically after this method.
  /// </summary>
  /// <example>
  /// protected override void Seed(MyDbContext context)
  /// {
  ///     context.Users.Add(new User { Id = 1, Name = "John" });
  /// }
  /// </example>
  protected virtual void Seed(TContext context)
  {
    // Default: no seeding
  }

  /// <summary>
  /// Creates a new DbContext instance with the same connection.
  /// Useful for testing detached scenarios.
  /// </summary>
  protected TContext CreateNewContext()
  {
    EnsureInitialized();
    var optionsBuilder = new DbContextOptionsBuilder<TContext>();
    _provider!.Configure(optionsBuilder);
    return CreateContext(optionsBuilder.Options);
  }

  /// <summary>
  /// Resets the database (deletes all data and recreates schema).
  /// </summary>
  protected void ResetDatabase()
  {
    EnsureInitialized();
    _context!.Database.EnsureDeleted();
    _context.Database.EnsureCreated();
    Seed(_context);
    if (_context.ChangeTracker.HasChanges())
    {
      _context.SaveChanges();
    }
  }

  /// <summary>
  /// Clears the change tracker (detaches all entities).
  /// </summary>
  protected void ClearChangeTracker()
  {
    EnsureInitialized();
    _context!.ChangeTracker.Clear();
  }

  /// <summary>
  /// Detaches an entity from the change tracker.
  /// </summary>
  protected void Detach<TEntity>(TEntity entity) where TEntity : class
  {
    EnsureInitialized();
    _context!.Entry(entity).State = EntityState.Detached;
  }

  /// <summary>
  /// Reloads an entity from the database.
  /// </summary>
  protected void Reload<TEntity>(TEntity entity) where TEntity : class
  {
    EnsureInitialized();
    _context!.Entry(entity).Reload();
  }

  /// <summary>
  /// Reloads an entity from the database asynchronously.
  /// </summary>
  protected Task ReloadAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
          where TEntity : class
  {
    EnsureInitialized();
    return _context!.Entry(entity).ReloadAsync(cancellationToken);
  }

  /// <summary>
  /// Creates a DbContext instance using reflection.
  /// </summary>
  private TContext CreateContext(DbContextOptions<TContext> options)
  {
    var constructor = typeof(TContext).GetConstructor(new[] { typeof(DbContextOptions<TContext>) });
    if (constructor != null)
    {
      return (TContext)constructor.Invoke(new object[] { options });
    }

    constructor = typeof(TContext).GetConstructor(new[] { typeof(DbContextOptions) });
    if (constructor != null)
    {
      return (TContext)constructor.Invoke(new object[] { options });
    }

    throw new InvalidOperationException(
            $"DbContext type '{typeof(TContext).Name}' must have a constructor that accepts DbContextOptions<{typeof(TContext).Name}> or DbContextOptions.");
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  public async ValueTask DisposeAsync()
  {
    await DisposeAsyncCore();
    Dispose(false);
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (!_disposed && disposing)
    {
      _context?.Dispose();
      _provider?.Dispose();
      _disposed = true;
    }
  }

  protected virtual async ValueTask DisposeAsyncCore()
  {
    if (!_disposed)
    {
      if (_context != null)
        await _context.DisposeAsync();
      _provider?.Dispose();
      _disposed = true;
    }
  }
}
