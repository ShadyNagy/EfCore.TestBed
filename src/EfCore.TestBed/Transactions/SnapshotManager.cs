using Microsoft.EntityFrameworkCore;

namespace EfCore.TestBed.Transactions;

/// <summary>
/// Manages database snapshots for testing scenarios.
/// Allows saving and restoring database state.
/// </summary>
public class SnapshotManager<TContext> : IDisposable
  where TContext : DbContext
{
  private readonly TContext _context;
  private readonly Dictionary<string, Snapshot> _snapshots = new();

  public SnapshotManager(TContext context)
  {
    _context = context;
  }

  /// <summary>
  /// Takes a snapshot of the current change tracker state.
  /// </summary>
  public void TakeSnapshot(string name = "default")
  {
    var entities = new List<EntitySnapshot>();

    foreach (var entry in _context.ChangeTracker.Entries())
    {
      if (entry.State != EntityState.Detached)
      {
        entities.Add(new EntitySnapshot(
          entry.Entity,
          entry.State,
          entry.CurrentValues.Clone(),
          entry.OriginalValues.Clone()
        ));
      }
    }

    _snapshots[name] = new Snapshot(entities);
  }

  /// <summary>
  /// Restores the change tracker to a previously saved snapshot.
  /// </summary>
  public void RestoreSnapshot(string name = "default")
  {
    if (!_snapshots.TryGetValue(name, out var snapshot))
    {
      throw new InvalidOperationException($"Snapshot '{name}' does not exist.");
    }

    // Detach all current entities
    foreach (var entry in _context.ChangeTracker.Entries().ToList())
    {
      entry.State = EntityState.Detached;
    }

    // Restore entities from snapshot
    foreach (var entitySnapshot in snapshot.Entities)
    {
      var entry = _context.Entry(entitySnapshot.Entity);
      entry.CurrentValues.SetValues(entitySnapshot.CurrentValues);
      entry.OriginalValues.SetValues(entitySnapshot.OriginalValues);
      entry.State = entitySnapshot.State;
    }
  }

  /// <summary>
  /// Checks if a snapshot exists.
  /// </summary>
  public bool HasSnapshot(string name = "default")
  {
    return _snapshots.ContainsKey(name);
  }

  /// <summary>
  /// Deletes a snapshot.
  /// </summary>
  public void DeleteSnapshot(string name = "default")
  {
    _snapshots.Remove(name);
  }

  /// <summary>
  /// Clears all snapshots.
  /// </summary>
  public void ClearSnapshots()
  {
    _snapshots.Clear();
  }

  /// <summary>
  /// Gets the names of all existing snapshots.
  /// </summary>
  public IEnumerable<string> GetSnapshotNames()
  {
    return _snapshots.Keys;
  }

  public void Dispose()
  {
    _snapshots.Clear();
  }

  private record Snapshot(List<EntitySnapshot> Entities);

  private record EntitySnapshot(
    object Entity,
    EntityState State,
    Microsoft.EntityFrameworkCore.ChangeTracking.PropertyValues CurrentValues,
    Microsoft.EntityFrameworkCore.ChangeTracking.PropertyValues OriginalValues
  );
}

/// <summary>
/// Extension methods for snapshot management.
/// </summary>
public static class SnapshotExtensions
{
  /// <summary>
  /// Creates a snapshot manager for the context.
  /// </summary>
  public static SnapshotManager<TContext> CreateSnapshotManager<TContext>(this TContext context)
    where TContext : DbContext
  {
    return new SnapshotManager<TContext>(context);
  }
}
