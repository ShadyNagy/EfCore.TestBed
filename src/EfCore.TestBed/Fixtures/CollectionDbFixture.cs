using Microsoft.EntityFrameworkCore;

namespace EfCore.TestBed.Fixtures;

/// <summary>
/// Collection fixture for sharing database across test classes.
/// Use with xUnit's [CollectionDefinition] and [Collection] attributes.
/// </summary>
public class CollectionDbFixture<TContext> : SharedDbFixture<TContext>
  where TContext : DbContext
{
}