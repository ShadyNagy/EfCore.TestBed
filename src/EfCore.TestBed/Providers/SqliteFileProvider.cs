using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EfCore.TestBed.Providers;

/// <summary>
/// SQLite file-based database provider.
/// </summary>
public class SqliteFileProvider : ITestDbProvider
{
  private readonly string _connectionString;
  private readonly bool _deleteOnDispose;
  private readonly string? _filePath;
  private bool _disposed;

  public bool SupportsTransactions => true;
  public bool ValidatesForeignKeys => true;

  public SqliteFileProvider(string? filePath = null, bool deleteOnDispose = true)
  {
    _filePath = filePath ?? Path.Combine(Path.GetTempPath(), $"testbed_{Guid.NewGuid()}.db");
    _connectionString = $"Data Source={_filePath}";
    _deleteOnDispose = deleteOnDispose;
  }

  public void Configure(DbContextOptionsBuilder builder)
  {
    builder.UseSqlite(_connectionString);
  }

  public void Dispose()
  {
    if (!_disposed && _deleteOnDispose && _filePath != null && File.Exists(_filePath))
    {
      try
      {
        SqliteConnection.ClearAllPools();
        File.Delete(_filePath);
      }
      catch
      {
        // Ignore cleanup errors
      }
      _disposed = true;
    }
  }
}
