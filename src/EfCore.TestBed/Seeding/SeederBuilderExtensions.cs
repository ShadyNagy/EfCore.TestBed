using Microsoft.EntityFrameworkCore;

namespace EfCore.TestBed.Seeding;

/// <summary>
/// Extension to create a seeder builder.
/// </summary>
public static class SeederBuilderExtensions
{
  /// <summary>
  /// Creates a seeder builder for fluent seeding.
  /// </summary>
  public static SeederBuilder<TContext> Seed<TContext>(this TContext context)
      where TContext : DbContext
  {
    return new SeederBuilder<TContext>(context);
  }
}
