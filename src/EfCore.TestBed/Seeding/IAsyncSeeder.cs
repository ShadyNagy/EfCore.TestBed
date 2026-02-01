using Microsoft.EntityFrameworkCore;

namespace EfCore.TestBed.Seeding;

/// <summary>
/// Interface for async seeders.
/// </summary>
public interface IAsyncSeeder<TContext> where TContext : DbContext
{
  /// <summary>
  /// Seeds data into the context asynchronously.
  /// </summary>
  Task SeedAsync(TContext context, CancellationToken cancellationToken = default);
}
