using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EfCore.TestBed.Transactions;

/// <summary>
/// Provides transaction helpers for testing scenarios.
/// </summary>
public static class TransactionHelper
{
  /// <summary>
  /// Executes an action within a transaction that is rolled back after completion.
  /// Useful for tests that should not persist data.
  /// </summary>
  public static void InRollbackTransaction<TContext>(this TContext context, Action<TContext> action)
      where TContext : DbContext
  {
    using var transaction = context.Database.BeginTransaction();
    try
    {
      action(context);
    }
    finally
    {
      transaction.Rollback();
    }
  }

  /// <summary>
  /// Executes an action within a transaction that is rolled back after completion (async).
  /// </summary>
  public static async Task InRollbackTransactionAsync<TContext>(this TContext context, Func<TContext, Task> action, CancellationToken cancellationToken = default)
      where TContext : DbContext
  {
    using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
    try
    {
      await action(context);
    }
    finally
    {
      await transaction.RollbackAsync(cancellationToken);
    }
  }

  /// <summary>
  /// Executes an action within a transaction.
  /// Transaction is committed if action succeeds, rolled back if it throws.
  /// </summary>
  public static void InTransaction<TContext>(this TContext context, Action<TContext> action)
      where TContext : DbContext
  {
    using var transaction = context.Database.BeginTransaction();
    try
    {
      action(context);
      transaction.Commit();
    }
    catch
    {
      transaction.Rollback();
      throw;
    }
  }

  /// <summary>
  /// Executes an action within a transaction (async).
  /// </summary>
  public static async Task InTransactionAsync<TContext>(this TContext context, Func<TContext, Task> action, CancellationToken cancellationToken = default)
      where TContext : DbContext
  {
    using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
    try
    {
      await action(context);
      await transaction.CommitAsync(cancellationToken);
    }
    catch
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }

  /// <summary>
  /// Executes a function within a transaction and returns the result.
  /// </summary>
  public static TResult InTransaction<TContext, TResult>(this TContext context, Func<TContext, TResult> func)
      where TContext : DbContext
  {
    using var transaction = context.Database.BeginTransaction();
    try
    {
      var result = func(context);
      transaction.Commit();
      return result;
    }
    catch
    {
      transaction.Rollback();
      throw;
    }
  }

  /// <summary>
  /// Executes a function within a transaction and returns the result (async).
  /// </summary>
  public static async Task<TResult> InTransactionAsync<TContext, TResult>(this TContext context, Func<TContext, Task<TResult>> func, CancellationToken cancellationToken = default)
      where TContext : DbContext
  {
    using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
    try
    {
      var result = await func(context);
      await transaction.CommitAsync(cancellationToken);
      return result;
    }
    catch
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}

/// <summary>
/// A scope that automatically rolls back changes when disposed.
/// </summary>
public class RollbackScope : IDisposable, IAsyncDisposable
{
  private readonly DbContext _context;
  private readonly IDbContextTransaction _transaction;
  private bool _disposed;
  private bool _committed;

  public RollbackScope(DbContext context)
  {
    _context = context;
    _transaction = context.Database.BeginTransaction();
  }

  /// <summary>
  /// Commits the transaction (prevents rollback on dispose).
  /// </summary>
  public void Commit()
  {
    _transaction.Commit();
    _committed = true;
  }

  /// <summary>
  /// Commits the transaction asynchronously.
  /// </summary>
  public async Task CommitAsync(CancellationToken cancellationToken = default)
  {
    await _transaction.CommitAsync(cancellationToken);
    _committed = true;
  }

  /// <summary>
  /// Explicitly rolls back the transaction.
  /// </summary>
  public void Rollback()
  {
    if (!_committed)
    {
      _transaction.Rollback();
    }
  }

  /// <summary>
  /// Explicitly rolls back the transaction asynchronously.
  /// </summary>
  public async Task RollbackAsync(CancellationToken cancellationToken = default)
  {
    if (!_committed)
    {
      await _transaction.RollbackAsync(cancellationToken);
    }
  }

  public void Dispose()
  {
    if (!_disposed)
    {
      if (!_committed)
      {
        try
        {
          _transaction.Rollback();
        }
        catch
        {
          // Ignore errors during dispose
        }
      }
      _transaction.Dispose();
      _disposed = true;
    }
  }

  public async ValueTask DisposeAsync()
  {
    if (!_disposed)
    {
      if (!_committed)
      {
        try
        {
          await _transaction.RollbackAsync();
        }
        catch
        {
          // Ignore errors during dispose
        }
      }
      await _transaction.DisposeAsync();
      _disposed = true;
    }
  }
}

/// <summary>
/// Extension methods for creating rollback scopes.
/// </summary>
public static class RollbackScopeExtensions
{
  /// <summary>
  /// Creates a rollback scope that will automatically rollback on dispose.
  /// </summary>
  public static RollbackScope CreateRollbackScope(this DbContext context)
  {
    return new RollbackScope(context);
  }
}
