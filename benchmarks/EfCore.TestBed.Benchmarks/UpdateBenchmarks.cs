using BenchmarkDotNet.Attributes;
using EfCore.TestBed.Benchmarks.Entities;
using EfCore.TestBed.Factory;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EfCore.TestBed.Benchmarks;

[MemoryDiagnoser]
[RankColumn]
public class UpdateBenchmarks
{
    [Params(1, 10, 100)]
    public int EntityCount { get; set; }

    [Benchmark(Description = "EfCore.TestBed (SQLite InMemory)")]
    public void TestBed_SqliteInMemory()
    {
        using var db = TestDb.Create<BenchmarkDbContext>(ctx =>
        {
            for (int i = 0; i < EntityCount; i++)
            {
                ctx.Users.Add(new User
                {
                    Name = $"User {i}",
                    Email = $"user{i}@test.com"
                });
            }
        });

        var users = db.Context.Users.ToList();
        foreach (var user in users)
        {
            user.Name = $"Updated {user.Name}";
        }
        db.Context.SaveChanges();
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

                for (int i = 0; i < EntityCount; i++)
                {
                    context.Users.Add(new User
                    {
                        Name = $"User {i}",
                        Email = $"user{i}@test.com"
                    });
                }
                context.SaveChanges();

                var users = context.Users.ToList();
                foreach (var user in users)
                {
                    user.Name = $"Updated {user.Name}";
                }
                context.SaveChanges();
            }

            SqliteConnection.ClearPool(new SqliteConnection(connectionString));
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Benchmark(Description = "EF Core InMemory")]
    public void EfCore_InMemory()
    {
        var options = new DbContextOptionsBuilder<BenchmarkDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new BenchmarkDbContext(options);
        context.Database.EnsureCreated();

        for (int i = 0; i < EntityCount; i++)
        {
            context.Users.Add(new User
            {
                Name = $"User {i}",
                Email = $"user{i}@test.com"
            });
        }
        context.SaveChanges();

        var users = context.Users.ToList();
        foreach (var user in users)
        {
            user.Name = $"Updated {user.Name}";
        }
        context.SaveChanges();
    }
}
