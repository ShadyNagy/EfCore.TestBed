using Microsoft.EntityFrameworkCore;

namespace EfCore.TestBed.Providers;

/// <summary>
/// EF Core InMemory provider.
/// </summary>
public class InMemoryProvider : ITestDbProvider
{
  private readonly string _databaseName;

  public bool SupportsTransactions => false;
  public bool ValidatesForeignKeys => false;

  public InMemoryProvider(string? databaseName = null)
  {
    _databaseName = databaseName ?? Guid.NewGuid().ToString();
  }

  public void Configure(DbContextOptionsBuilder builder)
  {
    builder.UseInMemoryDatabase(_databaseName);
  }

  public void Dispose()
  {
    // Nothing to dispose for InMemory
  }
}
