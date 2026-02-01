using Microsoft.EntityFrameworkCore;

namespace EfCore.TestBed.Providers;

/// <summary>
/// Interface for test database providers.
/// </summary>
public interface ITestDbProvider : IDisposable
{
  /// <summary>
  /// Configures the DbContext options builder.
  /// </summary>
  void Configure(DbContextOptionsBuilder builder);

  /// <summary>
  /// Gets whether this provider supports real transactions.
  /// </summary>
  bool SupportsTransactions { get; }

  /// <summary>
  /// Gets whether this provider validates foreign keys.
  /// </summary>
  bool ValidatesForeignKeys { get; }
}
