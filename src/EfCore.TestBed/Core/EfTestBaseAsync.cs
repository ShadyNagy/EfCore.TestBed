using Microsoft.EntityFrameworkCore;
using EfCore.TestBed.Configuration;
using EfCore.TestBed.Providers;

namespace EfCore.TestBed.Core;

/// <summary>
/// Async-friendly base class for EF Core tests.
/// Use this when you need async seeding or setup.
/// </summary>
/// <typeparam name="TContext">The DbContext type</typeparam>
public abstract class EfTestBaseAsync<TContext> : IAsyncDisposable, IDisposable
        where TContext : DbContext
{
  private ITestDbProvider? _provider;
  private TContext? _context;
  private bool _disposed;
  private bool _initialized;

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
  /// Initializes the test database asynchronously.
  /// Call this in your test setup method (e.g., [SetUp] or constructor).
  /// </summary>
  /// <example>
  /// [SetUp]
  /// public async Task Setup()
  /// {
  ///     await InitializeAsync();
  /// }
  /// </example>
  protected async Task InitializeAsync()
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
      await _context.Database.EnsureCreatedAsync();
    }

    await SeedAsync(_context);
    if (_context.ChangeTracker.HasChanges())
    {
      await _context.SaveChangesAsync();
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
  /// Override this method to seed test data asynchronously.
  /// SaveChanges is called automatically after this method.
  /// </summary>
  /// <example>
  /// protected override async Task SeedAsync(MyDbContext context)
  /// {
  ///     context.Users.Add(new User { Id = 1, Name = "John" });
  ///     await Task.CompletedTask;
  /// }
  /// </example>
  protected virtual Task SeedAsync(TContext context) => Task.CompletedTask;

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
  /// Resets the database asynchronously (deletes all data and recreates schema).
  /// </summary>
  protected async Task ResetDatabaseAsync()
  {
    EnsureInitialized();
    await _context!.Database.EnsureDeletedAsync();
    await _context.Database.EnsureCreatedAsync();
    await SeedAsync(_context);
    if (_context.ChangeTracker.HasChanges())
    {
      await _context.SaveChangesAsync();
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
  /// Reloads an entity from the database asynchronously.
  /// </summary>
  protected Task ReloadAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
          where TEntity : class
  {
    EnsureInitialized();
    return _context!.Entry(entity).ReloadAsync(cancellationToken);
  }

  private void EnsureInitialized()
  {
    if (!_initialized)
    {
      throw new InvalidOperationException(
              "Test database not initialized. Call InitializeAsync() in your test setup method.");
    }
  }

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
    if (!_disposed)
    {
      _context?.Dispose();
      _provider?.Dispose();
      _disposed = true;
    }
    GC.SuppressFinalize(this);
  }

  public async ValueTask DisposeAsync()
  {
    if (!_disposed)
    {
      if (_context != null)
        await _context.DisposeAsync();
      _provider?.Dispose();
      _disposed = true;
    }
    GC.SuppressFinalize(this);
  }
}
