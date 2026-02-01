using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using EfCore.TestBed.Assertions;

namespace EfCore.TestBed.Extensions;

/// <summary>
/// Extension methods for DbContext to simplify testing.
/// </summary>
public static class DbContextExtensions
{

  /// <summary>
  /// Asserts that an entity exists matching the predicate.
  /// </summary>
  public static void ShouldHave<TEntity>(this DbContext context, Expression<Func<TEntity, bool>> predicate)
    where TEntity : class
  {
    DbAssert.Exists(context, predicate);
  }

  /// <summary>
  /// Asserts that an entity exists matching the predicate (async).
  /// </summary>
  public static Task ShouldHaveAsync<TEntity>(this DbContext context, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    where TEntity : class
  {
    return DbAssert.ExistsAsync(context, predicate, cancellationToken);
  }

  /// <summary>
  /// Asserts that no entity exists matching the predicate.
  /// </summary>
  public static void ShouldNotHave<TEntity>(this DbContext context, Expression<Func<TEntity, bool>> predicate)
    where TEntity : class
  {
    DbAssert.NotExists(context, predicate);
  }

  /// <summary>
  /// Asserts that no entity exists matching the predicate (async).
  /// </summary>
  public static Task ShouldNotHaveAsync<TEntity>(this DbContext context, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    where TEntity : class
  {
    return DbAssert.NotExistsAsync(context, predicate, cancellationToken);
  }

  /// <summary>
  /// Asserts that the entity count matches expected.
  /// </summary>
  public static void ShouldHaveCount<TEntity>(this DbContext context, int expectedCount)
    where TEntity : class
  {
    DbAssert.Count<TEntity>(context, expectedCount);
  }

  /// <summary>
  /// Asserts that the entity count matches expected with predicate.
  /// </summary>
  public static void ShouldHaveCount<TEntity>(this DbContext context, int expectedCount, Expression<Func<TEntity, bool>> predicate)
    where TEntity : class
  {
    DbAssert.Count(context, expectedCount, predicate);
  }

  /// <summary>
  /// Asserts that SaveChanges will succeed.
  /// </summary>
  public static void ShouldSaveSuccessfully(this DbContext context)
  {
    DbAssert.SaveSucceeds(context);
  }

  /// <summary>
  /// Asserts that SaveChanges will fail with specific exception.
  /// </summary>
  public static TException ShouldFailOnSave<TException>(this DbContext context)
    where TException : Exception
  {
    return DbAssert.SaveFails<TException>(context);
  }

  /// <summary>
  /// Asserts that SaveChanges will fail (any exception).
  /// </summary>
  public static Exception ShouldFailOnSave(this DbContext context)
  {
    return DbAssert.SaveFails(context);
  }

  /// <summary>
  /// Clears all entities of a type from the database.
  /// </summary>
  public static void ClearTable<TEntity>(this DbContext context)
    where TEntity : class
  {
    context.Set<TEntity>().RemoveRange(context.Set<TEntity>());
    context.SaveChanges();
  }

  /// <summary>
  /// Clears all entities of a type from the database (async).
  /// </summary>
  public static async Task ClearTableAsync<TEntity>(this DbContext context, CancellationToken cancellationToken = default)
    where TEntity : class
  {
    context.Set<TEntity>().RemoveRange(context.Set<TEntity>());
    await context.SaveChangesAsync(cancellationToken);
  }

  /// <summary>
  /// Clears the change tracker (detaches all entities).
  /// </summary>
  public static void ClearTracking(this DbContext context)
  {
    context.ChangeTracker.Clear();
  }

  /// <summary>
  /// Detaches an entity from the change tracker.
  /// </summary>
  public static void Detach<TEntity>(this DbContext context, TEntity entity)
    where TEntity : class
  {
    context.Entry(entity).State = EntityState.Detached;
  }

  /// <summary>
  /// Detaches all entities of a type from the change tracker.
  /// </summary>
  public static void DetachAll<TEntity>(this DbContext context)
    where TEntity : class
  {
    foreach (var entry in context.ChangeTracker.Entries<TEntity>().ToList())
    {
      entry.State = EntityState.Detached;
    }
  }

  /// <summary>
  /// Gets the count of entities in the change tracker by state.
  /// </summary>
  public static int GetTrackedCount(this DbContext context, EntityState state)
  {
    return context.ChangeTracker.Entries().Count(e => e.State == state);
  }

  /// <summary>
  /// Gets all entities in the change tracker with a specific state.
  /// </summary>
  public static IEnumerable<TEntity> GetTracked<TEntity>(this DbContext context, EntityState state)
    where TEntity : class
  {
    return context.ChangeTracker.Entries<TEntity>()
      .Where(e => e.State == state)
      .Select(e => e.Entity);
  }

  /// <summary>
  /// Finds and returns an entity, or throws if not found.
  /// </summary>
  public static TEntity FindOrThrow<TEntity>(this DbContext context, params object[] keyValues)
    where TEntity : class
  {
    var entity = context.Set<TEntity>().Find(keyValues);
    if (entity == null)
    {
      throw new InvalidOperationException($"Entity of type '{typeof(TEntity).Name}' with key [{string.Join(", ", keyValues)}] not found.");
    }
    return entity;
  }

  /// <summary>
  /// Finds and returns an entity, or throws if not found (async).
  /// </summary>
  public static async Task<TEntity> FindOrThrowAsync<TEntity>(this DbContext context, params object[] keyValues)
    where TEntity : class
  {
    var entity = await context.Set<TEntity>().FindAsync(keyValues);
    if (entity == null)
    {
      throw new InvalidOperationException($"Entity of type '{typeof(TEntity).Name}' with key [{string.Join(", ", keyValues)}] not found.");
    }
    return entity;
  }

  /// <summary>
  /// Adds and immediately saves an entity.
  /// </summary>
  public static TEntity AddAndSave<TEntity>(this DbContext context, TEntity entity)
    where TEntity : class
  {
    context.Set<TEntity>().Add(entity);
    context.SaveChanges();
    return entity;
  }

  /// <summary>
  /// Adds and immediately saves an entity (async).
  /// </summary>
  public static async Task<TEntity> AddAndSaveAsync<TEntity>(this DbContext context, TEntity entity, CancellationToken cancellationToken = default)
    where TEntity : class
  {
    context.Set<TEntity>().Add(entity);
    await context.SaveChangesAsync(cancellationToken);
    return entity;
  }

  /// <summary>
  /// Adds multiple entities and saves.
  /// </summary>
  public static void AddRangeAndSave<TEntity>(this DbContext context, params TEntity[] entities)
    where TEntity : class
  {
    context.Set<TEntity>().AddRange(entities);
    context.SaveChanges();
  }

  /// <summary>
  /// Removes an entity and saves.
  /// </summary>
  public static void RemoveAndSave<TEntity>(this DbContext context, TEntity entity)
    where TEntity : class
  {
    context.Set<TEntity>().Remove(entity);
    context.SaveChanges();
  }

  /// <summary>
  /// Gets a fresh copy of an entity from the database (bypassing cache).
  /// </summary>
  public static TEntity? GetFresh<TEntity>(this DbContext context, params object[] keyValues)
    where TEntity : class
  {
    var entity = context.Set<TEntity>().Find(keyValues);
    if (entity != null)
    {
      context.Entry(entity).State = EntityState.Detached;
    }
    return context.Set<TEntity>().Find(keyValues);
  }

  /// <summary>
  /// Gets the first entity matching predicate, or throws.
  /// </summary>
  public static TEntity FirstOrThrow<TEntity>(this DbContext context, Expression<Func<TEntity, bool>> predicate)
    where TEntity : class
  {
    var entity = context.Set<TEntity>().FirstOrDefault(predicate);
    if (entity == null)
    {
      throw new InvalidOperationException($"No entity of type '{typeof(TEntity).Name}' matches the predicate.");
    }
    return entity;
  }

  /// <summary>
  /// Gets the single entity matching predicate, or throws.
  /// </summary>
  public static TEntity SingleOrThrow<TEntity>(this DbContext context, Expression<Func<TEntity, bool>> predicate)
    where TEntity : class
  {
    return context.Set<TEntity>().Single(predicate);
  }
}
