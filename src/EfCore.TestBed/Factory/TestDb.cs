using EfCore.TestBed.Configuration;
using EfCore.TestBed.Providers;
using Microsoft.EntityFrameworkCore;
using QueryTrackingBehavior = EfCore.TestBed.Configuration.QueryTrackingBehavior;

namespace EfCore.TestBed.Factory;

/// <summary>
/// Factory for creating test database contexts.
/// </summary>
public static class TestDb
{
  /// <summary>
  /// Creates a new test DbContext with SQLite in-memory (default).
  /// </summary>
  /// <typeparam name="TContext">The DbContext type</typeparam>
  /// <param name="seed">Optional action to seed data</param>
  /// <param name="configure">Optional action to configure options</param>
  /// <returns>A wrapper containing the context and connection</returns>
  public static TestDbContext<TContext> Create<TContext>(
    Action<TContext>? seed = null,
    Action<TestBedOptions>? configure = null)
    where TContext : DbContext
  {
    var options = new TestBedOptions();
    configure?.Invoke(options);

    return CreateWithOptions<TContext>(options, seed);
  }

  /// <summary>
  /// Creates a new test DbContext with SQLite in-memory using async seeding.
  /// </summary>
  public static async Task<TestDbContext<TContext>> CreateAsync<TContext>(
    Func<TContext, Task>? seedAsync = null,
    Action<TestBedOptions>? configure = null)
    where TContext : DbContext
  {
    var options = new TestBedOptions();
    configure?.Invoke(options);

    var wrapper = CreateWithOptions<TContext>(options, null);

    if (seedAsync != null)
    {
      await seedAsync(wrapper.Context);
      await wrapper.Context.SaveChangesAsync();
    }

    return wrapper;
  }

  /// <summary>
  /// Creates a test DbContext with specified provider.
  /// </summary>
  public static TestDbContext<TContext> CreateWithProvider<TContext>(
    TestDbProvider provider,
    Action<TContext>? seed = null)
    where TContext : DbContext
  {
    var options = new TestBedOptions { Provider = provider };
    return CreateWithOptions<TContext>(options, seed);
  }

  /// <summary>
  /// Creates a quick disposable context (use with 'using' statement).
  /// </summary>
  public static TContext Quick<TContext>(Action<TContext>? seed = null)
    where TContext : DbContext
  {
    var wrapper = Create<TContext>(seed);
    return wrapper.Context;
  }

  private static TestDbContext<TContext> CreateWithOptions<TContext>(
    TestBedOptions options,
    Action<TContext>? seed)
    where TContext : DbContext
  {
    var provider = TestDbProviderFactory.Create(options.Provider, options);

    var optionsBuilder = new DbContextOptionsBuilder<TContext>();
    provider.Configure(optionsBuilder);

    if (options.EnableSensitiveDataLogging)
      optionsBuilder.EnableSensitiveDataLogging();

    if (options.EnableDetailedErrors)
      optionsBuilder.EnableDetailedErrors();

    optionsBuilder.UseQueryTrackingBehavior(options.QueryTrackingBehavior switch
    {
      QueryTrackingBehavior.NoTracking => Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking,
      QueryTrackingBehavior.NoTrackingWithIdentityResolution => Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTrackingWithIdentityResolution,
      _ => Microsoft.EntityFrameworkCore.QueryTrackingBehavior.TrackAll
    });

    var context = CreateContextInstance<TContext>(optionsBuilder.Options);

    if (options.AutoCreateDatabase)
    {
      context.Database.EnsureCreated();
    }

    if (seed != null)
    {
      seed(context);
      context.SaveChanges();
    }

    return new TestDbContext<TContext>(context, provider);
  }

  private static TContext CreateContextInstance<TContext>(DbContextOptions<TContext> options)
    where TContext : DbContext
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
}

/// <summary>
/// Wrapper class that holds the DbContext and manages the connection lifecycle.
/// </summary>
public class TestDbContext<TContext> : IDisposable, IAsyncDisposable
  where TContext : DbContext
{
  private readonly ITestDbProvider _provider;
  private bool _disposed;

  /// <summary>
  /// The DbContext instance.
  /// </summary>
  public TContext Context { get; }

  /// <summary>
  /// Gets whether the provider supports real transactions.
  /// </summary>
  public bool SupportsTransactions => _provider.SupportsTransactions;

  /// <summary>
  /// Gets whether the provider validates foreign keys.
  /// </summary>
  public bool ValidatesForeignKeys => _provider.ValidatesForeignKeys;

  internal TestDbContext(TContext context, ITestDbProvider provider)
  {
    Context = context;
    _provider = provider;
  }

  /// <summary>
  /// Implicit conversion to TContext for convenience.
  /// </summary>
  public static implicit operator TContext(TestDbContext<TContext> wrapper) => wrapper.Context;

  /// <summary>
  /// Resets the database (deletes all data and recreates schema).
  /// </summary>
  public void Reset()
  {
    Context.Database.EnsureDeleted();
    Context.Database.EnsureCreated();
  }

  /// <summary>
  /// Resets the database asynchronously.
  /// </summary>
  public async Task ResetAsync(CancellationToken cancellationToken = default)
  {
    await Context.Database.EnsureDeletedAsync(cancellationToken);
    await Context.Database.EnsureCreatedAsync(cancellationToken);
  }

  /// <summary>
  /// Clears the change tracker.
  /// </summary>
  public void ClearChangeTracker()
  {
    Context.ChangeTracker.Clear();
  }

  public void Dispose()
  {
    if (!_disposed)
    {
      Context.Dispose();
      _provider.Dispose();
      _disposed = true;
    }
  }

  public async ValueTask DisposeAsync()
  {
    if (!_disposed)
    {
      await Context.DisposeAsync();
      _provider.Dispose();
      _disposed = true;
    }
  }
}
