using BenchmarkDotNet.Attributes;
using EfCore.TestBed.Benchmarks.Entities;
using EfCore.TestBed.Factory;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EfCore.TestBed.Benchmarks;

[MemoryDiagnoser]
[RankColumn]
public class TransactionBenchmarks
{
  [Params(10, 50)]
  public int OperationCount { get; set; }

  [Benchmark(Description = "EfCore.TestBed (SQLite InMemory)")]
  public void TestBed_SqliteInMemory()
  {
    using var db = TestDb.Create<BenchmarkDbContext>();

    using var transaction = db.Context.Database.BeginTransaction();
    try
    {
      for (int i = 0; i < OperationCount; i++)
      {
        db.Context.Users.Add(new User
        {
          Name = $"User {i}",
          Email = $"user{i}@test.com"
        });
        db.Context.SaveChanges();
      }
      transaction.Commit();
    }
    catch
    {
      transaction.Rollback();
      throw;
    }
  }

  [Benchmark(Description = "SQLite Physical (File)")]
  public void Sqlite_Physical()
  {
    var dbPath = Path.Combine(Path.GetTempPath(), $"benchmark_{Guid.NewGuid()}.db");
    var connectionString = $"Data Source={dbPath}";
    try
    {
      var options = new DbContextOptionsBuilder<BenchmarkDbContext>()
          .UseSqlite(connectionString)
          .Options;

      using (var context = new BenchmarkDbContext(options))
      {
        context.Database.EnsureCreated();

        using var transaction = context.Database.BeginTransaction();
        try
        {
          for (int i = 0; i < OperationCount; i++)
          {
            context.Users.Add(new User
            {
              Name = $"User {i}",
              Email = $"user{i}@test.com"
            });
            context.SaveChanges();
          }
          transaction.Commit();
        }
        catch
        {
          transaction.Rollback();
          throw;
        }
      }

      SqliteConnection.ClearPool(new SqliteConnection(connectionString));
    }
    finally
    {
      if (File.Exists(dbPath))
        File.Delete(dbPath);
    }
  }

  [Benchmark(Description = "EF Core InMemory (No Real Transactions)")]
  public void EfCore_InMemory()
  {
    var options = new DbContextOptionsBuilder<BenchmarkDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
        .Options;

    using var context = new BenchmarkDbContext(options);
    context.Database.EnsureCreated();

    using var transaction = context.Database.BeginTransaction();
    try
    {
      for (int i = 0; i < OperationCount; i++)
      {
        context.Users.Add(new User
        {
          Name = $"User {i}",
          Email = $"user{i}@test.com"
        });
        context.SaveChanges();
      }
      transaction.Commit();
    }
    catch
    {
      transaction.Rollback();
      throw;
    }
  }
}
