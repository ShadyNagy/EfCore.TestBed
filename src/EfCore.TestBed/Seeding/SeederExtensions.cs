using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace EfCore.TestBed.Seeding;

/// <summary>
/// Extension methods for seeding data.
/// </summary>
public static class SeederExtensions
{
  /// <summary>
  /// Seeds data using a seeder instance.
  /// </summary>
  public static TContext SeedWith<TContext>(this TContext context, ISeeder<TContext> seeder)
      where TContext : DbContext
  {
    seeder.Seed(context);
    context.SaveChanges();
    return context;
  }

  /// <summary>
  /// Seeds data using a seeder instance (async).
  /// </summary>
  public static async Task<TContext> SeedWithAsync<TContext>(this TContext context, IAsyncSeeder<TContext> seeder, CancellationToken cancellationToken = default)
      where TContext : DbContext
  {
    await seeder.SeedAsync(context, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);
    return context;
  }

  /// <summary>
  /// Seeds data using an action.
  /// </summary>
  public static TContext SeedWith<TContext>(this TContext context, Action<TContext> seedAction)
      where TContext : DbContext
  {
    seedAction(context);
    context.SaveChanges();
    return context;
  }

  /// <summary>
  /// Seeds data using an async function.
  /// </summary>
  public static async Task<TContext> SeedWithAsync<TContext>(this TContext context, Func<TContext, Task> seedAction, CancellationToken cancellationToken = default)
      where TContext : DbContext
  {
    await seedAction(context);
    await context.SaveChangesAsync(cancellationToken);
    return context;
  }

  /// <summary>
  /// Seeds a single entity and returns it.
  /// </summary>
  public static TEntity SeedOne<TEntity>(this DbContext context, TEntity entity)
      where TEntity : class
  {
    context.Set<TEntity>().Add(entity);
    context.SaveChanges();
    return entity;
  }

  /// <summary>
  /// Seeds a single entity and returns it (async).
  /// </summary>
  public static async Task<TEntity> SeedOneAsync<TEntity>(this DbContext context, TEntity entity, CancellationToken cancellationToken = default)
      where TEntity : class
  {
    context.Set<TEntity>().Add(entity);
    await context.SaveChangesAsync(cancellationToken);
    return entity;
  }

  /// <summary>
  /// Seeds multiple entities and returns them.
  /// </summary>
  public static IReadOnlyList<TEntity> SeedMany<TEntity>(this DbContext context, params TEntity[] entities)
      where TEntity : class
  {
    context.Set<TEntity>().AddRange(entities);
    context.SaveChanges();
    return entities;
  }

  /// <summary>
  /// Seeds multiple entities and returns them.
  /// </summary>
  public static IReadOnlyList<TEntity> SeedMany<TEntity>(this DbContext context, IEnumerable<TEntity> entities)
      where TEntity : class
  {
    var entityList = entities.ToList();
    context.Set<TEntity>().AddRange(entityList);
    context.SaveChanges();
    return entityList;
  }

  /// <summary>
  /// Seeds multiple entities using a factory function.
  /// </summary>
  public static IReadOnlyList<TEntity> SeedMany<TEntity>(this DbContext context, int count, Func<int, TEntity> factory)
      where TEntity : class
  {
    var entities = Enumerable.Range(0, count).Select(factory).ToList();
    context.Set<TEntity>().AddRange(entities);
    context.SaveChanges();
    return entities;
  }

  /// <summary>
  /// Seeds entities if the table is empty.
  /// </summary>
  public static void SeedIfEmpty<TEntity>(this DbContext context, params TEntity[] entities)
      where TEntity : class
  {
    if (!context.Set<TEntity>().Any())
    {
      context.Set<TEntity>().AddRange(entities);
      context.SaveChanges();
    }
  }

  /// <summary>
  /// Seeds entities if the table is empty (async).
  /// </summary>
  public static async Task SeedIfEmptyAsync<TEntity>(this DbContext context, CancellationToken cancellationToken, params TEntity[] entities)
      where TEntity : class
  {
    if (!await context.Set<TEntity>().AnyAsync(cancellationToken))
    {
      context.Set<TEntity>().AddRange(entities);
      await context.SaveChangesAsync(cancellationToken);
    }
  }
}
