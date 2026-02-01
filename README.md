# EfCore.TestBed 🧪

The ultimate EF Core testing toolkit. Write tests with **real database behavior** in milliseconds.

[![NuGet](https://img.shields.io/nuget/v/EfCore.TestBed.svg)](https://www.nuget.org/packages/EfCore.TestBed/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## 🚀 Why EfCore.TestBed?

| Standard InMemory | EfCore.TestBed |
|-------------------|----------------|
| ❌ No FK validation | ✅ Real FK validation |
| ❌ No unique constraints | ✅ Real unique constraints |
| ❌ No cascade delete | ✅ Real cascade delete |
| ❌ No transactions | ✅ Real transactions |
| ❌ No raw SQL | ✅ Real SQL support |
| Same speed | Same speed |

**One line of code = real database testing** 🎯

## 📦 Installation

```bash
dotnet add package EfCore.TestBed
```

## ⚡ Quick Start

### Option 1: Inherit from EfTestBase (Recommended)

```csharp
using EfCore.TestBed.Core;

public class OrderTests : EfTestBase<MyDbContext>
{
    protected override void Seed(MyDbContext context)
    {
        context.Users.Add(new User { Id = 1, Name = "John" });
    }

    [Fact]
    public void CreateOrder_WithValidUser_Succeeds()
    {
        // Db is ready with seeded data!
        Db.Orders.Add(new Order { UserId = 1 });
        Db.SaveChanges();

        Assert.Equal(1, Db.Orders.Count());
    }

    [Fact]
    public void CreateOrder_WithInvalidUser_Fails()
    {
        Db.Orders.Add(new Order { UserId = 999 }); // User doesn't exist!

        Assert.Throws<DbUpdateException>(() => Db.SaveChanges()); // ✅ Real FK error!
    }
}
```

### Option 2: Use Factory Method

```csharp
using EfCore.TestBed.Factory;

[Fact]
public void QuickTest()
{
    using var db = TestDb.Create<MyDbContext>(ctx =>
    {
        ctx.Users.Add(new User { Name = "Test" });
    });

    Assert.Equal(1, db.Context.Users.Count());
}
```

### Option 3: One-liner

```csharp
[Fact]
public void SuperQuickTest()
{
    using var db = TestDb.Quick<MyDbContext>();
    db.Users.Add(new User { Name = "John" });
    db.SaveChanges();
}
```

## 📚 Features

### 🧪 Base Class for Tests

```csharp
public class MyTests : EfTestBase<AppDbContext>
{
    // Fresh database for each test!
    
    protected override void Seed(AppDbContext context)
    {
        // Optional: seed test data
        context.Users.Add(new User { Name = "Test User" });
    }

    [Fact]
    public void MyTest()
    {
        // Db property is ready to use
        var user = Db.Users.First();
        Assert.Equal("Test User", user.Name);
    }
}
```

### ✅ Fluent Assertions

```csharp
using EfCore.TestBed.Extensions;

// Check existence
Db.ShouldHave<User>(u => u.Name == "John");
Db.ShouldNotHave<Order>(o => o.Status == "Cancelled");

// Check counts
Db.ShouldHaveCount<User>(5);
Db.ShouldHaveCount<Order>(3, o => o.Status == "Pending");

// Check save operations
Db.ShouldSaveSuccessfully();
Db.ShouldFailOnSave<DbUpdateException>();
```

### 🔄 Transaction Support

```csharp
using EfCore.TestBed.Transactions;

// Auto-rollback after test
Db.InRollbackTransaction(ctx =>
{
    ctx.Users.Add(new User { Name = "Temp" });
    ctx.SaveChanges();
    // Rolled back automatically!
});

// Manual rollback scope
using var scope = Db.CreateRollbackScope();
Db.Users.Add(new User { Name = "Test" });
Db.SaveChanges();
scope.Rollback(); // Data is gone!
```

### 🌱 Easy Seeding

```csharp
using EfCore.TestBed.Seeding;

// Seed single entity
var user = Db.SeedOne(new User { Name = "John" });

// Seed multiple
var users = Db.SeedMany(
    new User { Name = "John" },
    new User { Name = "Jane" }
);

// Seed with factory
var products = Db.SeedMany(10, i => new Product 
{ 
    Name = $"Product {i}",
    Price = 9.99m * i
});

// Fluent seeding
Db.Seed()
  .Add(new User { Name = "John" })
  .Add(5, i => new Order { Total = i * 10 })
  .Build();
```

### 🏗️ Shared Fixtures (xUnit)

```csharp
// Define a shared fixture
public class MyDbFixture : SharedDbFixture<AppDbContext>
{
    protected override void SeedData(AppDbContext context)
    {
        context.Users.Add(new User { Name = "Shared User" });
    }
}

// Use in test class
public class MyTests : IClassFixture<MyDbFixture>
{
    private readonly AppDbContext _db;

    public MyTests(MyDbFixture fixture)
    {
        _db = fixture.CreateContext();
    }

    [Fact]
    public void Test1() => Assert.NotEmpty(_db.Users);
}
```

### 📸 Snapshots

```csharp
using EfCore.TestBed.Transactions;

var snapshot = Db.CreateSnapshotManager();

// Take snapshot
snapshot.TakeSnapshot("before");

// Make changes
Db.Users.First().Name = "Changed";

// Restore
snapshot.RestoreSnapshot("before");
```

### 🔧 Configuration

```csharp
// Custom options
public class MyTests : EfTestBase<AppDbContext>
{
    public MyTests() : base(new TestBedOptions
    {
        Provider = TestDbProvider.SqliteInMemory, // Default
        EnableSensitiveDataLogging = true,
        EnableDetailedErrors = true
    })
    {
    }
}

// Or via factory
using var db = TestDb.Create<AppDbContext>(configure: opts =>
{
    opts.Provider = TestDbProvider.SqliteInMemory;
    opts.EnableSensitiveDataLogging = true;
});
```

## 🎯 Complete Example

```csharp
using EfCore.TestBed.Core;
using EfCore.TestBed.Extensions;
using EfCore.TestBed.Seeding;

public class OrderServiceTests : EfTestBase<AppDbContext>
{
    protected override void Seed(AppDbContext context)
    {
        // Seed test users
        context.SeedMany(
            new User { Id = 1, Name = "John", Email = "john@test.com" },
            new User { Id = 2, Name = "Jane", Email = "jane@test.com" }
        );

        // Seed products
        context.SeedMany(3, i => new Product
        {
            Id = i + 1,
            Name = $"Product {i + 1}",
            Price = 10.00m * (i + 1)
        });
    }

    [Fact]
    public void CreateOrder_WithValidData_CreatesOrderWithItems()
    {
        // Arrange
        var service = new OrderService(Db);
        
        // Act
        var order = service.CreateOrder(userId: 1, productIds: new[] { 1, 2 });
        
        // Assert
        Db.ShouldHave<Order>(o => o.UserId == 1);
        Db.ShouldHaveCount<OrderItem>(2, oi => oi.OrderId == order.Id);
        Assert.Equal(30.00m, order.Total); // 10 + 20
    }

    [Fact]
    public void CreateOrder_WithInvalidUser_ThrowsException()
    {
        var service = new OrderService(Db);

        Assert.Throws<DbUpdateException>(() => 
            service.CreateOrder(userId: 999, productIds: new[] { 1 }));
    }

    [Fact]
    public void DeleteUser_WithOrders_CascadeDeletes()
    {
        // Arrange
        var order = Db.SeedOne(new Order { UserId = 1, Total = 100 });
        
        // Act
        var user = Db.Users.Find(1)!;
        Db.Users.Remove(user);
        Db.SaveChanges();
        
        // Assert
        Db.ShouldNotHave<User>(u => u.Id == 1);
        Db.ShouldNotHave<Order>(o => o.Id == order.Id); // Cascade deleted!
    }

    [Fact]
    public async Task TransactionRollback_WorksCorrectly()
    {
        // Arrange
        var initialCount = Db.Orders.Count();

        // Act - Add order in transaction then rollback
        await Db.InRollbackTransactionAsync(async ctx =>
        {
            ctx.Orders.Add(new Order { UserId = 1, Total = 50 });
            await ctx.SaveChangesAsync();
        });

        // Assert - Count should be unchanged
        Assert.Equal(initialCount, Db.Orders.Count());
    }
}
```

## 📋 Requirements

- .NET 8.0+
- EF Core 8.0+

## 🤝 Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## 📄 License

MIT License - feel free to use in your projects!

---

**Made with ❤️ for the .NET community**
