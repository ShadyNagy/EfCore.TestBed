using Microsoft.EntityFrameworkCore;

namespace EfCore.TestBed.Seeding;

/// <summary>
/// Interface for defining seeders.
/// </summary>
public interface ISeeder<TContext> where TContext : DbContext
{
  /// <summary>
  /// Seeds data into the context.
  /// </summary>
  void Seed(TContext context);
}
