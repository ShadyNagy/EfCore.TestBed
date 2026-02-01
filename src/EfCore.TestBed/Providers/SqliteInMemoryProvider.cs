using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EfCore.TestBed.Providers;

/// <summary>
/// SQLite in-memory database provider.
/// </summary>
public class SqliteInMemoryProvider : ITestDbProvider
{
  private readonly SqliteConnection _connection;
  private bool _disposed;

  public bool SupportsTransactions => true;
  public bool ValidatesForeignKeys => true;

  public SqliteInMemoryProvider()
  {
    _connection = new SqliteConnection("Data Source=:memory:");
    _connection.Open();

    // Enable foreign keys
    using var command = _connection.CreateCommand();
    command.CommandText = "PRAGMA foreign_keys = ON;";
    command.ExecuteNonQuery();
  }

  public void Configure(DbContextOptionsBuilder builder)
  {
    builder.UseSqlite(_connection);
  }

  public void Dispose()
  {
    if (!_disposed)
    {
      _connection.Close();
      _connection.Dispose();
      _disposed = true;
    }
  }
}
