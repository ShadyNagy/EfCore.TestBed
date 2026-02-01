using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EfCore.TestBed.Assertions;

/// <summary>
/// Assertion helper methods for database testing.
/// </summary>
public static class DbAssert
{
  /// <summary>
  /// Asserts that an entity exists matching the predicate.
  /// </summary>
  public static void Exists<TEntity>(DbContext context, Expression<Func<TEntity, bool>> predicate)
    where TEntity : class
  {
    var exists = context.Set<TEntity>().Any(predicate);
    if (!exists)
    {
      throw new AssertionException($"Expected entity of type '{typeof(TEntity).Name}' matching predicate to exist, but none was found.");
    }
  }

  /// <summary>
  /// Asserts that an entity exists matching the predicate (async).
  /// </summary>
  public static async Task ExistsAsync<TEntity>(DbContext context, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    where TEntity : class
  {
    var exists = await context.Set<TEntity>().AnyAsync(predicate, cancellationToken);
    if (!exists)
    {
      throw new AssertionException($"Expected entity of type '{typeof(TEntity).Name}' matching predicate to exist, but none was found.");
    }
  }

  /// <summary>
  /// Asserts that no entity exists matching the predicate.
  /// </summary>
  public static void NotExists<TEntity>(DbContext context, Expression<Func<TEntity, bool>> predicate)
    where TEntity : class
  {
    var exists = context.Set<TEntity>().Any(predicate);
    if (exists)
    {
      throw new AssertionException($"Expected no entity of type '{typeof(TEntity).Name}' matching predicate to exist, but one was found.");
    }
  }

  /// <summary>
  /// Asserts that no entity exists matching the predicate (async).
  /// </summary>
  public static async Task NotExistsAsync<TEntity>(DbContext context, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    where TEntity : class
  {
    var exists = await context.Set<TEntity>().AnyAsync(predicate, cancellationToken);
    if (exists)
    {
      throw new AssertionException($"Expected no entity of type '{typeof(TEntity).Name}' matching predicate to exist, but one was found.");
    }
  }

  /// <summary>
  /// Asserts that the entity count matches expected.
  /// </summary>
  public static void Count<TEntity>(DbContext context, int expectedCount)
    where TEntity : class
  {
    var actualCount = context.Set<TEntity>().Count();
    if (actualCount != expectedCount)
    {
      throw new AssertionException($"Expected {expectedCount} entities of type '{typeof(TEntity).Name}', but found {actualCount}.");
    }
  }

  /// <summary>
  /// Asserts that the entity count matches expected with predicate.
  /// </summary>
  public static void Count<TEntity>(DbContext context, int expectedCount, Expression<Func<TEntity, bool>> predicate)
    where TEntity : class
  {
    var actualCount = context.Set<TEntity>().Count(predicate);
    if (actualCount != expectedCount)
    {
      throw new AssertionException($"Expected {expectedCount} entities of type '{typeof(TEntity).Name}' matching predicate, but found {actualCount}.");
    }
  }

  /// <summary>
  /// Asserts that the DbSet is empty.
  /// </summary>
  public static void Empty<TEntity>(DbContext context)
    where TEntity : class
  {
    var count = context.Set<TEntity>().Count();
    if (count != 0)
    {
      throw new AssertionException($"Expected no entities of type '{typeof(TEntity).Name}', but found {count}.");
    }
  }

  /// <summary>
  /// Asserts that the DbSet is not empty.
  /// </summary>
  public static void NotEmpty<TEntity>(DbContext context)
    where TEntity : class
  {
    var count = context.Set<TEntity>().Count();
    if (count == 0)
    {
      throw new AssertionException($"Expected at least one entity of type '{typeof(TEntity).Name}', but found none.");
    }
  }

  /// <summary>
  /// Asserts that SaveChanges will succeed.
  /// </summary>
  public static void SaveSucceeds(DbContext context)
  {
    try
    {
      context.SaveChanges();
    }
    catch (Exception ex)
    {
      throw new AssertionException($"Expected SaveChanges to succeed, but it threw: {ex.Message}", ex);
    }
  }

  /// <summary>
  /// Asserts that SaveChanges will fail with specific exception type.
  /// </summary>
  public static TException SaveFails<TException>(DbContext context)
    where TException : Exception
  {
    try
    {
      context.SaveChanges();
      throw new AssertionException($"Expected SaveChanges to throw {typeof(TException).Name}, but it succeeded.");
    }
    catch (TException ex)
    {
      return ex;
    }
    catch (Exception ex)
    {
      throw new AssertionException($"Expected SaveChanges to throw {typeof(TException).Name}, but it threw {ex.GetType().Name}: {ex.Message}", ex);
    }
  }

  /// <summary>
  /// Asserts that SaveChanges will fail (any exception).
  /// </summary>
  public static Exception SaveFails(DbContext context)
  {
    try
    {
      context.SaveChanges();
      throw new AssertionException("Expected SaveChanges to fail, but it succeeded.");
    }
    catch (AssertionException)
    {
      throw;
    }
    catch (Exception ex)
    {
      return ex;
    }
  }

  /// <summary>
  /// Asserts that an entity has been modified.
  /// </summary>
  public static void IsModified<TEntity>(DbContext context, TEntity entity)
    where TEntity : class
  {
    var state = context.Entry(entity).State;
    if (state != EntityState.Modified)
    {
      throw new AssertionException($"Expected entity to be Modified, but state was {state}.");
    }
  }

  /// <summary>
  /// Asserts that an entity has been added.
  /// </summary>
  public static void IsAdded<TEntity>(DbContext context, TEntity entity)
    where TEntity : class
  {
    var state = context.Entry(entity).State;
    if (state != EntityState.Added)
    {
      throw new AssertionException($"Expected entity to be Added, but state was {state}.");
    }
  }

  /// <summary>
  /// Asserts that an entity has been deleted.
  /// </summary>
  public static void IsDeleted<TEntity>(DbContext context, TEntity entity)
    where TEntity : class
  {
    var state = context.Entry(entity).State;
    if (state != EntityState.Deleted)
    {
      throw new AssertionException($"Expected entity to be Deleted, but state was {state}.");
    }
  }

  /// <summary>
  /// Asserts that an entity is unchanged.
  /// </summary>
  public static void IsUnchanged<TEntity>(DbContext context, TEntity entity)
    where TEntity : class
  {
    var state = context.Entry(entity).State;
    if (state != EntityState.Unchanged)
    {
      throw new AssertionException($"Expected entity to be Unchanged, but state was {state}.");
    }
  }

  /// <summary>
  /// Asserts that an entity is detached.
  /// </summary>
  public static void IsDetached<TEntity>(DbContext context, TEntity entity)
    where TEntity : class
  {
    var state = context.Entry(entity).State;
    if (state != EntityState.Detached)
    {
      throw new AssertionException($"Expected entity to be Detached, but state was {state}.");
    }
  }
}
