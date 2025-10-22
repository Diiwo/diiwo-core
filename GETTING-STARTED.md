# Getting Started with Diiwo.Core

This guide will walk you through setting up and using **Diiwo.Core** in your .NET application, from basic setup to advanced enterprise scenarios.

## 🎯 What You'll Achieve

By the end of this guide, you'll have:

- ✅ **Zero-code audit trails** - Automatic tracking of CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- ✅ **Enterprise soft delete** - Data preservation for compliance requirements
- ✅ **Entity state management** - Complete lifecycle tracking
- ✅ **User attribution** - Know exactly who made what changes when
- ✅ **Production-ready setup** - Same patterns used in **Diiwo.Identity** enterprise systems

## 📋 Prerequisites

- .NET 8.0 or later
- Entity Framework Core 8.0 or later
- Your favorite IDE (Visual Studio, VS Code, Rider)

## 🚀 Installation

### Package Manager Console
```powershell
Install-Package Diiwo.Core
```

### .NET CLI
```bash
dotnet add package Diiwo.Core
```

### PackageReference
```xml
<PackageReference Include="Diiwo.Core" Version="0.1.0" />
```

## 🏗️ Basic Setup (5 Minutes)

### Step 1: Create Your Entities

```csharp
using Diiwo.Core.Domain.Entities;

// Simple product entity
public class Product : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

// Order with user tracking
public class Order : UserTrackedEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = "Pending";
    
    // Navigation properties
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem : AuditableEntity
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    
    // Navigation properties
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
```

### Step 2: Create Your CurrentUserService

For **ASP.NET Core** applications:

```csharp
using Diiwo.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public class WebCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WebCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public string? UserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name;

    public string? UserEmail => _httpContextAccessor.HttpContext?.User?
        .FindFirst(ClaimTypes.Email)?.Value;

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    
    public Task<bool> IsInRoleAsync(string role)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return Task.FromResult(user?.IsInRole(role) ?? false);
    }
}
```

For **Console/Service** applications:

```csharp
public class SystemCurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; } = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public string? UserName => "System";
    public string? UserEmail => "system@diiwo.com";
    public bool IsAuthenticated => true;
    public Task<bool> IsInRoleAsync(string role) => Task.FromResult(true);
}
```

### Step 3: Setup DbContext

```csharp
using Diiwo.Core.Interceptors;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    private readonly AuditInterceptor _auditInterceptor;

    public AppDbContext(DbContextOptions<AppDbContext> options, AuditInterceptor auditInterceptor) 
        : base(options)
    {
        _auditInterceptor = auditInterceptor;
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure your entities
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.Property(e => e.Total).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(e => e.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasOne(oi => oi.Order)
                  .WithMany(o => o.Items)
                  .HasForeignKey(oi => oi.OrderId);

            entity.HasOne(oi => oi.Product)
                  .WithMany()
                  .HasForeignKey(oi => oi.ProductId);

            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
        });

        // Apply soft delete query filter for all AuditableEntity types
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IAuditable).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(e => EF.Property<EntityState>(e, nameof(IAuditable.State)) != EntityState.Terminated);
            }
        }
    }
}
```

### Step 4: Register Services

**For ASP.NET Core:**

```csharp
// Program.cs
using Diiwo.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, WebCurrentUserService>();

// Add Entity Framework
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Diiwo.Core with existing user service
builder.Services.AddDiiwoCoreWithExistingUserService();

var app = builder.Build();
```

**For Console Applications:**

```csharp
// Program.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Diiwo.Core.Extensions;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Add Entity Framework
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection")));

        // Add Diiwo.Core with system user service
        services.AddDiiwoCore<SystemCurrentUserService>();
        
        // Add your application services
        services.AddScoped<IProductService, ProductService>();
    })
    .Build();

await host.RunAsync();
```

## 💡 Usage Examples

### Creating and Saving Entities

```csharp
public class ProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Product> CreateProductAsync(string name, decimal price, string description, string category)
    {
        var product = new Product
        {
            Name = name,
            Price = price,
            Description = description,
            Category = category
            // Id, CreatedAt, UpdatedAt, State, CreatedBy, UpdatedBy automatically set!
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return product;
    }

    public async Task<List<Product>> GetActiveProductsAsync()
    {
        return await _context.Products
            .Where(p => p.IsActive) // Built-in property from AuditableEntity
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Product?> UpdateProductAsync(Guid id, string name, decimal price)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return null;

        product.Name = name;
        product.Price = price;
        // UpdatedAt and UpdatedBy automatically set on SaveChanges!

        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<bool> SoftDeleteProductAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return false;

        product.SoftDelete(); // Sets State = EntityState.Terminated
        // UpdatedAt and UpdatedBy automatically set!
        
        await _context.SaveChangesAsync();
        return true;
    }
}
```

### Working with User-Tracked Entities

```csharp
public class OrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Order> CreateOrderAsync(List<(Guid ProductId, int Quantity)> items)
    {
        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            OrderDate = DateTime.UtcNow,
            Status = "Pending"
        };

        decimal total = 0;
        foreach (var (productId, quantity) in items)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) continue;

            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = product.Price
            };

            total += orderItem.UnitPrice * orderItem.Quantity;
            order.Items.Add(orderItem);
        }

        order.Total = total;
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // All entities now have:
        // - Id automatically generated
        // - CreatedAt = DateTime.UtcNow
        // - UpdatedAt = DateTime.UtcNow  
        // - CreatedBy = current user ID (from ICurrentUserService)
        // - UpdatedBy = current user ID
        // - State = EntityState.Active

        return order;
    }

    private string GenerateOrderNumber() => $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
}
```

### Querying with Audit Information

```csharp
public class ReportService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ReportService(AppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<ProductAuditInfo>> GetProductAuditReportAsync()
    {
        return await _context.Products
            .Select(p => new ProductAuditInfo
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                CreatedBy = p.CreatedBy,
                UpdatedBy = p.UpdatedBy,
                State = p.State,
                IsActive = p.IsActive,
                DaysSinceCreated = (DateTime.UtcNow - p.CreatedAt).Days
            })
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetMyOrdersAsync()
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
            return new List<Order>();

        return await _context.Orders
            .Where(o => o.CreatedBy == _currentUserService.UserId.Value)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<List<object>> GetRecentActivityAsync(int days = 7)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);

        var productActivity = await _context.Products
            .Where(p => p.UpdatedAt >= cutoffDate)
            .Select(p => new
            {
                Type = "Product",
                Id = p.Id,
                Name = p.Name,
                Action = p.CreatedAt >= cutoffDate ? "Created" : "Updated",
                Timestamp = p.UpdatedAt,
                UserId = p.UpdatedBy
            })
            .ToListAsync();

        var orderActivity = await _context.Orders
            .Where(o => o.UpdatedAt >= cutoffDate)
            .Select(o => new
            {
                Type = "Order",
                Id = o.Id,
                Name = o.OrderNumber,
                Action = o.CreatedAt >= cutoffDate ? "Created" : "Updated",
                Timestamp = o.UpdatedAt,
                UserId = o.UpdatedBy
            })
            .ToListAsync();

        return productActivity.Cast<object>()
            .Concat(orderActivity.Cast<object>())
            .OrderByDescending(x => ((dynamic)x).Timestamp)
            .ToList();
    }
}

public class ProductAuditInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public EntityState State { get; set; }
    public bool IsActive { get; set; }
    public int DaysSinceCreated { get; set; }
}
```

## 🔧 Advanced Scenarios

### Custom Entity States

```csharp
public class Document : UserTrackedEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";

    public void Publish()
    {
        Status = "Published";
        State = EntityState.Effective; // Use Effective for published content
    }

    public void Archive()
    {
        Status = "Archived";
        State = EntityState.Inactive; // Use Inactive for archived content
    }

    public bool IsPublished => State == EntityState.Effective && Status == "Published";
    public bool IsArchived => State == EntityState.Inactive && Status == "Archived";
}
```

### Task Assignment with State Management

```csharp
public class TaskItem : UserTrackedEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public Guid? AssignedTo { get; set; }

    public void AssignTo(Guid userId)
    {
        AssignedTo = userId;
        Status = "In Progress";
        State = EntityState.Active;
    }

    public void Complete()
    {
        Status = "Completed";
        State = EntityState.Effective; // Use Effective for completed tasks
    }

    public bool CanEdit(Guid? currentUserId)
    {
        if (!IsActive && State != EntityState.Effective) return false;
        if (State == EntityState.Terminated) return false;

        // Only assigned user or unassigned tasks can be edited
        return AssignedTo == null || AssignedTo == currentUserId;
    }
}
```

### Multi-Tenant with UserOwnedEntity

```csharp
public class Note : UserOwnedEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsPublic { get; set; } = false;
    public List<string> Tags { get; set; } = new();
}

public class NoteService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public NoteService(AppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<Note>> GetMyNotesAsync()
    {
        if (!_currentUserService.UserId.HasValue)
            return new List<Note>();

        return await _context.Notes
            .Where(n => n.IsOwnedBy(_currentUserService.UserId.Value))
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync();
    }

    public async Task<List<Note>> GetPublicNotesAsync()
    {
        return await _context.Notes
            .Where(n => n.IsPublic && n.IsActive)
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync();
    }

    public async Task<Note> CreateNoteAsync(string title, string content, bool isPublic = false)
    {
        var note = new Note
        {
            Title = title,
            Content = content,
            IsPublic = isPublic
            // UserId automatically set by UserOwnedEntity if current user is available
        };

        _context.Notes.Add(note);
        await _context.SaveChangesAsync();

        return note;
    }
}
```

## 🧪 Testing

### Unit Testing Your Entities

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class ProductTests
{
    [TestMethod]
    public void Product_ShouldInheritAuditProperties()
    {
        // Arrange & Act
        var product = new Product
        {
            Name = "Test Product",
            Price = 99.99m,
            Description = "Test Description",
            Category = "Test"
        };

        // Assert - Properties from AuditableEntity
        Assert.AreNotEqual(Guid.Empty, product.Id);
        Assert.IsTrue(product.CreatedAt > DateTime.MinValue);
        Assert.IsTrue(product.UpdatedAt > DateTime.MinValue);
        Assert.AreEqual(EntityState.Active, product.State);
        Assert.IsTrue(product.IsActive);
        Assert.IsFalse(product.IsTerminated);
    }

    [TestMethod]
    public void Product_SoftDelete_ShouldSetStateToTerminated()
    {
        // Arrange
        var product = new Product { Name = "Test", Price = 1, Description = "Test", Category = "Test" };
        var originalUpdatedAt = product.UpdatedAt;

        // Act
        product.SoftDelete();

        // Assert
        Assert.AreEqual(EntityState.Terminated, product.State);
        Assert.IsTrue(product.IsTerminated);
        Assert.IsFalse(product.IsActive);
        Assert.IsTrue(product.UpdatedAt > originalUpdatedAt);
    }
}
```

### Integration Testing with In-Memory Database

```csharp
[TestClass]
public class ProductServiceIntegrationTests
{
    private DbContextOptions<AppDbContext> GetInMemoryDbOptions()
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [TestMethod]
    public async Task CreateProduct_ShouldSetAuditFields()
    {
        // Arrange
        var options = GetInMemoryDbOptions();
        var currentUserService = new TestCurrentUserService();
        var auditInterceptor = new AuditInterceptor(currentUserService);

        await using var context = new AppDbContext(options, auditInterceptor);
        var productService = new ProductService(context);

        // Act
        var product = await productService.CreateProductAsync("Test Product", 99.99m, "Description", "Category");

        // Assert
        Assert.AreNotEqual(Guid.Empty, product.Id);
        Assert.IsTrue(product.CreatedAt > DateTime.MinValue);
        Assert.IsTrue(product.UpdatedAt > DateTime.MinValue);
        Assert.AreEqual(currentUserService.UserId, product.CreatedBy);
        Assert.AreEqual(currentUserService.UserId, product.UpdatedBy);
        Assert.AreEqual(EntityState.Active, product.State);
    }
}

public class TestCurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; } = Guid.Parse("12345678-1234-1234-1234-123456789012");
    public string? UserName => "TestUser";
    public bool IsAuthenticated => true;
    public Task<bool> IsInRoleAsync(string role) => Task.FromResult(true);
}
```

## 📈 Performance Optimization

### Query Filtering

```csharp
// Instead of loading all and filtering in memory
var activeProducts = await context.Products.ToListAsync();
var filtered = activeProducts.Where(p => p.IsActive);

// Use database-level filtering
var activeProducts = await context.Products
    .Where(p => p.IsActive)
    .ToListAsync();
```

### Projection for Reports

```csharp
// Instead of loading full entities for reports
var products = await context.Products.ToListAsync();
var report = products.Select(p => new { p.Name, p.Price, p.CreatedAt });

// Project at database level
var report = await context.Products
    .Select(p => new ProductSummary 
    { 
        Name = p.Name, 
        Price = p.Price, 
        CreatedAt = p.CreatedAt 
    })
    .ToListAsync();
```

### Batch Operations

```csharp
public async Task BulkUpdateStatusAsync(List<Guid> productIds, string status)
{
    await context.Products
        .Where(p => productIds.Contains(p.Id))
        .ExecuteUpdateAsync(p => p.SetProperty(x => x.Status, status));
    
    // UpdatedAt and UpdatedBy will be set by AuditInterceptor for tracked changes
}
```

## 🚀 Next Steps

Now that you have the basics working:

1. **Read the [Architecture Guide](ARCHITECTURE.md)** to understand design decisions
2. **Check out [Examples](Examples/)** for more complex scenarios
3. **Explore integration** with your favorite patterns (Repository, CQRS, etc.)
4. **Consider [Diiwo.Identity](https://github.com/diiwo/diiwo-identity)** for authentication and authorization

## ❓ Common Questions

**Q: Can I use this with existing entities?**  
A: Yes! Just change your base class to inherit from the appropriate Diiwo.Core entity.

**Q: What if I don't want all the audit fields?**  
A: Use `BaseEntity` for minimal fields, or create your own base by implementing the interfaces.

**Q: Can I customize the audit behavior?**  
A: Absolutely! Inherit from `AuditInterceptor` and override `UpdateAuditFields`.

**Q: Does this work with database-first approaches?**  
A: Yes, but you'll need to manually add the audit fields to your database schema and entities.

---

## 🆘 Need Help?

- 🐛 [Report Issues](https://github.com/diiwo/diiwo-core/issues)
- 💬 [Join Discussions](https://github.com/diiwo/diiwo-core/discussions)
- 📧 [Email Support](mailto:support@diiwo.com)