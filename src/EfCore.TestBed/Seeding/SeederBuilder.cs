using Microsoft.EntityFrameworkCore;

namespace EfCore.TestBed.Seeding;

/// <summary>
/// Fluent builder for seeding data.
/// </summary>
public class SeederBuilder<TContext> where TContext : DbContext
{
  private readonly TContext _context;
  private readonly List<Action<TContext>> _seedActions = new();

  public SeederBuilder(TContext context)
  {
    _context = context;
  }

  /// <summary>
  /// Adds entities to be seeded.
  /// </summary>
  public SeederBuilder<TContext> Add<TEntity>(params TEntity[] entities)
      where TEntity : class
  {
    _seedActions.Add(ctx => ctx.Set<TEntity>().AddRange(entities));
    return this;
  }

  /// <summary>
  /// Adds entities using a factory.
  /// </summary>
  public SeederBuilder<TContext> Add<TEntity>(int count, Func<int, TEntity> factory)
      where TEntity : class
  {
    _seedActions.Add(ctx =>
    {
      var entities = Enumerable.Range(0, count).Select(factory);
      ctx.Set<TEntity>().AddRange(entities);
    });
    return this;
  }

  /// <summary>
  /// Adds a custom seed action.
  /// </summary>
  public SeederBuilder<TContext> With(Action<TContext> action)
  {
    _seedActions.Add(action);
    return this;
  }

  /// <summary>
  /// Executes all seed actions and saves changes.
  /// </summary>
  public TContext Build()
  {
    foreach (var action in _seedActions)
    {
      action(_context);
    }
    _context.SaveChanges();
    return _context;
  }

  /// <summary>
  /// Executes all seed actions and saves changes (async).
  /// </summary>
  public async Task<TContext> BuildAsync(CancellationToken cancellationToken = default)
  {
    foreach (var action in _seedActions)
    {
      action(_context);
    }
    await _context.SaveChangesAsync(cancellationToken);
    return _context;
  }
}
