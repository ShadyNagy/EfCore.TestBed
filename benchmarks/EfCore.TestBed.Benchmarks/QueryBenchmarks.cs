using BenchmarkDotNet.Attributes;
using EfCore.TestBed.Benchmarks.Entities;
using EfCore.TestBed.Factory;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EfCore.TestBed.Benchmarks;

[MemoryDiagnoser]
[RankColumn]
public class QueryBenchmarks
{
    private const int SeedCount = 100;

    private TestDbContext<BenchmarkDbContext>? _testBedDb;
    private BenchmarkDbContext? _sqlitePhysicalContext;
    private BenchmarkDbContext? _inMemoryContext;
    private string? _sqlitePhysicalPath;

    [GlobalSetup]
    public void Setup()
    {
        _testBedDb = TestDb.Create<BenchmarkDbContext>(SeedData);

        _sqlitePhysicalPath = Path.Combine(Path.GetTempPath(), $"benchmark_query_{Guid.NewGuid()}.db");
        var sqliteOptions = new DbContextOptionsBuilder<BenchmarkDbContext>()
            .UseSqlite($"Data Source={_sqlitePhysicalPath}")
            .Options;
        _sqlitePhysicalContext = new BenchmarkDbContext(sqliteOptions);
        _sqlitePhysicalContext.Database.EnsureCreated();
        SeedData(_sqlitePhysicalContext);
        _sqlitePhysicalContext.SaveChanges();

        var inMemoryOptions = new DbContextOptionsBuilder<BenchmarkDbContext>()
            .UseInMemoryDatabase("QueryBenchmark")
            .Options;
        _inMemoryContext = new BenchmarkDbContext(inMemoryOptions);
        _inMemoryContext.Database.EnsureCreated();
        SeedData(_inMemoryContext);
        _inMemoryContext.SaveChanges();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _testBedDb?.Dispose();
        _sqlitePhysicalContext?.Dispose();
        _inMemoryContext?.Dispose();

        if (_sqlitePhysicalPath != null && File.Exists(_sqlitePhysicalPath))
            File.Delete(_sqlitePhysicalPath);
    }

    private static void SeedData(BenchmarkDbContext context)
    {
        for (int i = 0; i < SeedCount; i++)
        {
            var user = new User
            {
                Name = $"User {i}",
                Email = $"user{i}@test.com"
            };
            context.Users.Add(user);
        }

        for (int i = 0; i < SeedCount; i++)
        {
            context.Products.Add(new Product
            {
                Name = $"Product {i}",
                SKU = $"SKU-{i:D5}",
                Price = 10.00m + i,
                Stock = 100
            });
        }
    }

    [Benchmark(Description = "EfCore.TestBed (SQLite InMemory)")]
    public List<User> TestBed_SqliteInMemory_Query()
    {
        return _testBedDb!.Context.Users.Where(u => u.Name.Contains("5")).ToList();
    }

    [Benchmark(Description = "SQLite Physical (File)")]
    public List<User> Sqlite_Physical_Query()
    {
        return _sqlitePhysicalContext!.Users.Where(u => u.Name.Contains("5")).ToList();
    }

    [Benchmark(Description = "EF Core InMemory")]
    public List<User> EfCore_InMemory_Query()
    {
        return _inMemoryContext!.Users.Where(u => u.Name.Contains("5")).ToList();
    }
}
