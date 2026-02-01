using BenchmarkDotNet.Attributes;
using EfCore.TestBed.Benchmarks.Entities;
using EfCore.TestBed.Factory;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EfCore.TestBed.Benchmarks;

[MemoryDiagnoser]
[RankColumn]
public class ComplexQueryBenchmarks
{
    private const int UserCount = 50;
    private const int ProductCount = 100;
    private const int OrdersPerUser = 5;

    private TestDbContext<BenchmarkDbContext>? _testBedDb;
    private BenchmarkDbContext? _sqlitePhysicalContext;
    private BenchmarkDbContext? _inMemoryContext;
    private string? _sqlitePhysicalPath;

    [GlobalSetup]
    public void Setup()
    {
        _testBedDb = TestDb.Create<BenchmarkDbContext>(SeedComplexData);

        _sqlitePhysicalPath = Path.Combine(Path.GetTempPath(), $"benchmark_complex_{Guid.NewGuid()}.db");
        var sqliteOptions = new DbContextOptionsBuilder<BenchmarkDbContext>()
            .UseSqlite($"Data Source={_sqlitePhysicalPath}")
            .Options;
        _sqlitePhysicalContext = new BenchmarkDbContext(sqliteOptions);
        _sqlitePhysicalContext.Database.EnsureCreated();
        SeedComplexData(_sqlitePhysicalContext);
        _sqlitePhysicalContext.SaveChanges();
        _sqlitePhysicalContext.ChangeTracker.Clear();

        var inMemoryOptions = new DbContextOptionsBuilder<BenchmarkDbContext>()
            .UseInMemoryDatabase("ComplexQueryBenchmark")
            .Options;
        _inMemoryContext = new BenchmarkDbContext(inMemoryOptions);
        _inMemoryContext.Database.EnsureCreated();
        SeedComplexData(_inMemoryContext);
        _inMemoryContext.SaveChanges();
        _inMemoryContext.ChangeTracker.Clear();
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

    private static void SeedComplexData(BenchmarkDbContext context)
    {
        var products = new List<Product>();
        for (int i = 0; i < ProductCount; i++)
        {
            var product = new Product
            {
                Name = $"Product {i}",
                SKU = $"SKU-{i:D5}",
                Price = 10.00m + (i % 50),
                Stock = 100 + i
            };
            products.Add(product);
            context.Products.Add(product);
        }
        context.SaveChanges();

        for (int u = 0; u < UserCount; u++)
        {
            var user = new User
            {
                Name = $"User {u}",
                Email = $"user{u}@test.com"
            };
            context.Users.Add(user);
            context.SaveChanges();

            for (int o = 0; o < OrdersPerUser; o++)
            {
                var order = new Order
                {
                    UserId = user.Id,
                    Total = 0,
                    Status = o % 3 == 0 ? "Completed" : (o % 3 == 1 ? "Pending" : "Shipped")
                };
                context.Orders.Add(order);
                context.SaveChanges();

                decimal orderTotal = 0;
                for (int oi = 0; oi < 3; oi++)
                {
                    var product = products[(u * OrdersPerUser + o + oi) % ProductCount];
                    var item = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = product.Id,
                        Quantity = oi + 1,
                        UnitPrice = product.Price
                    };
                    orderTotal += item.Quantity * item.UnitPrice;
                    context.OrderItems.Add(item);
                }
                order.Total = orderTotal;
                context.SaveChanges();
            }
        }
    }

    [Benchmark(Description = "EfCore.TestBed - Join Query")]
    public List<object> TestBed_JoinQuery()
    {
        return _testBedDb!.Context.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Where(o => o.Total > 50)
            .Select(o => new { o.Id, UserName = o.User.Name, ItemCount = o.Items.Count })
            .ToList<object>();
    }

    [Benchmark(Description = "SQLite Physical - Join Query")]
    public List<object> Sqlite_JoinQuery()
    {
        return _sqlitePhysicalContext!.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Where(o => o.Total > 50)
            .Select(o => new { o.Id, UserName = o.User.Name, ItemCount = o.Items.Count })
            .ToList<object>();
    }

    [Benchmark(Description = "EF Core InMemory - Join Query")]
    public List<object> EfCore_JoinQuery()
    {
        return _inMemoryContext!.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Where(o => o.Total > 50)
            .Select(o => new { o.Id, UserName = o.User.Name, ItemCount = o.Items.Count })
            .ToList<object>();
    }

    [Benchmark(Description = "EfCore.TestBed - Aggregate Query")]
    public List<object> TestBed_AggregateQuery()
    {
        return _testBedDb!.Context.Users
            .Select(u => new
            {
                u.Name,
                OrderCount = u.Orders.Count,
                TotalSpent = u.Orders.Sum(o => o.Total)
            })
            .Where(x => x.OrderCount > 0)
            .OrderByDescending(x => x.TotalSpent)
            .ToList<object>();
    }

    [Benchmark(Description = "SQLite Physical - Aggregate Query")]
    public List<object> Sqlite_AggregateQuery()
    {
        return _sqlitePhysicalContext!.Users
            .Select(u => new
            {
                u.Name,
                OrderCount = u.Orders.Count,
                TotalSpent = u.Orders.Sum(o => o.Total)
            })
            .Where(x => x.OrderCount > 0)
            .OrderByDescending(x => x.TotalSpent)
            .ToList<object>();
    }

    [Benchmark(Description = "EF Core InMemory - Aggregate Query")]
    public List<object> EfCore_AggregateQuery()
    {
        return _inMemoryContext!.Users
            .Select(u => new
            {
                u.Name,
                OrderCount = u.Orders.Count,
                TotalSpent = u.Orders.Sum(o => o.Total)
            })
            .Where(x => x.OrderCount > 0)
            .OrderByDescending(x => x.TotalSpent)
            .ToList<object>();
    }
}
