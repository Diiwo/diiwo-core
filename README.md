# Diiwo.Core

[![Build Status](https://img.shields.io/github/actions/workflow/status/diiwo/diiwo-core/build.yml?branch=main)](https://github.com/diiwo/diiwo-core/actions
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

> **Universal base entities, audit trails, and core functionality for all .NET applications.**

**Diiwo.Core** is a lightweight, universal library that provides base entities, automatic audit trails, soft delete functionality, and essential interfaces for building robust .NET applications. Designed to be framework-agnostic and highly reusable across web, console, desktop, and service applications.

## ✨ Key Features

- 🏗️ **Universal Base Entities** - `BaseEntity`, `AuditableEntity`, `UserTrackedEntity`, `DomainEntity`
- 🔍 **Zero-Code Audit Trails** - Automatic audit field population with **zero manual code required**
- 🗑️ **Enterprise Soft Delete** - Preserve data for compliance while marking as terminated
- 📊 **Entity State Management** - Complete lifecycle tracking (`Created`, `Active`, `Inactive`, `Terminated`)
- 👤 **User Attribution** - Automatic tracking of who made changes and when
- 🏢 **Compliance Ready** - Meet regulatory requirements with comprehensive audit trails
- 🏛️ **Clean Architecture Ready** - Follows DDD and clean architecture principles
- 🌐 **Framework Agnostic** - Works with any .NET application type (Web, Console, Desktop, Services)
- ⚡ **Zero Dependencies** - Minimal external dependencies for maximum compatibility
- 🧪 **Production Proven** - Powers enterprise identity solutions like **Diiwo.Identity**

## 🚀 Quick Start

### Installation

#### Option 1: GitHub Packages (Recommended)
```bash
# Add GitHub Packages as a source
dotnet nuget add source https://nuget.pkg.github.com/Diiwo/index.json --name github --username YOUR_GITHUB_USERNAME --password YOUR_GITHUB_TOKEN

# Install via .NET CLI
dotnet add package Diiwo.Core --version 0.1.0
```

#### Option 2: Project Reference
```xml
<ProjectReference Include="..\DIIWO-Core\DIIWO.Core.csproj" />
```

#### Option 3: GitHub Release
Download the `.nupkg` file from [Releases](https://github.com/Diiwo/diiwo-core/releases) and install locally:
```bash
dotnet add package Diiwo.Core --source /path/to/downloaded/packages
```

### Basic Setup

```csharp
// Program.cs or Startup.cs
using Diiwo.Core.Extensions;

// Register Diiwo.Core with your CurrentUserService implementation
services.AddDiiwoCore<MyCurrentUserService>();

// Or if you already have ICurrentUserService registered elsewhere
services.AddDiiwoCoreWithExistingUserService();
```

### Your Entity

```csharp
using Diiwo.Core.Domain.Entities;

// Simple entity with basic audit fields
public class Product : AuditableEntity
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
    
    // IsActive, CreatedAt, UpdatedAt, State automatically included!
}

// Entity with user tracking
public class Order : UserTrackedEntity  
{
    public string OrderNumber { get; set; }
    public decimal Total { get; set; }
    
    // Includes all AuditableEntity fields PLUS CreatedBy, UpdatedBy
}

// Multi-tenant entity
public class Document : UserOwnedEntity
{
    public string Title { get; set; }
    public string Content { get; set; }
    
    // Includes UserTrackedEntity fields PLUS UserId for ownership
}
```

### DbContext Setup

```csharp
using Diiwo.Core.Interceptors;
using Microsoft.EntityFrameworkCore;

public class MyDbContext : DbContext
{
    private readonly AuditInterceptor _auditInterceptor;

    public MyDbContext(DbContextOptions<MyDbContext> options, AuditInterceptor auditInterceptor) 
        : base(options)
    {
        _auditInterceptor = auditInterceptor;
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Add audit interceptor for automatic audit trail population
        optionsBuilder.AddInterceptors(_auditInterceptor);
    }
}
```

### 🎯 Zero-Code Enterprise Audit Trail

```csharp
// ✅ Enterprise-grade auditing with ZERO manual code!
var product = new Product
{
    Name = "iPhone 15",
    Price = 999.00m,
    Description = "Latest iPhone"
    // ✅ NO audit code needed - everything automatic!
};

context.Products.Add(product);
await context.SaveChangesAsync();  // 🎯 Triggers AuditInterceptor

// ✅ Automatically populated by Diiwo.Core:
// product.Id = Guid.NewGuid()
// product.CreatedAt = DateTime.UtcNow
// product.UpdatedAt = DateTime.UtcNow
// product.State = EntityState.Active
// product.CreatedBy = currentUserId (from ICurrentUserService)
// product.UpdatedBy = currentUserId

// ✅ Enterprise benefits:
// - Complete audit trail for compliance
// - User attribution for security
// - State management for lifecycle tracking
// - Soft delete preserves data history
```

## 🏗️ Architecture Overview

### Base Entity Hierarchy

```
BaseEntity (Id, CreatedAt, UpdatedAt)
├── AuditableEntity (+ State, Soft Delete)
    ├── UserTrackedEntity (+ CreatedBy, UpdatedBy)
        ├── DomainEntity (for business entities)
        └── UserOwnedEntity (+ UserId for multi-tenant)
```

### Entity States

```csharp
public enum EntityState
{
    Created = 0,     // Created but not yet active
    Inactive = 1,    // Temporarily inactive  
    Active = 2,      // Active and available
    Effective = 3,   // Effective and operational
    Terminated = 4   // Soft deleted
}
```

### Entity State Management

Entities automatically transition through states during their lifecycle:

- **Created** - Initial state when entity is created
- **Inactive** - Temporarily disabled but can be reactivated
- **Active** - Normal operational state (default)
- **Effective** - Fully operational and effective
- **Terminated** - Soft deleted, preserves audit history

## 📖 Usage Examples

### Implementing ICurrentUserService

```csharp
// For ASP.NET Core applications
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

// For Console/Service applications  
public class SystemCurrentUserService : ICurrentUserService
{
    public Guid? UserId => Guid.Parse("00000000-0000-0000-0000-000000000001"); // System user
    public string? UserName => "System";
    public string? UserEmail => "system@diiwo.com";
    public bool IsAuthenticated => true;
    public Task<bool> IsInRoleAsync(string role) => Task.FromResult(true);
}
```

### Working with Soft Deletes

```csharp
// Instead of hard delete, mark as terminated
product.SoftDelete(); // Sets State = EntityState.Terminated
await context.SaveChangesAsync();

// Query only active entities
var activeProducts = await context.Products
    .Where(p => p.IsActive)  // Built-in property
    .ToListAsync();

// Include soft-deleted entities
var allProducts = await context.Products
    .IgnoreQueryFilters()
    .ToListAsync();

// Restore soft-deleted entity
product.Restore(); // Sets State = EntityState.Active  
await context.SaveChangesAsync();
```

### Entity State Control

```csharp
// Deactivate entity temporarily
product.State = EntityState.Inactive;
await context.SaveChangesAsync();

// Check state in your business logic
if (!product.IsActive)
{
    throw new InvalidOperationException("Product is not available");
}

// Reactivate entity
product.State = EntityState.Active;
await context.SaveChangesAsync();
```

### Multi-Tenant Usage

```csharp
// Create user-owned entity
var document = new Document 
{
    Title = "My Document",
    Content = "...",
    UserId = currentUserId  // Automatically set by UserOwnedEntity
};

// Check ownership
if (document.IsOwnedBy(currentUserId))
{
    // User can access this document
}

// Query user's documents
var userDocs = await context.Documents
    .Where(d => d.UserId == currentUserId || d.IsGlobal)
    .ToListAsync();
```

## 🔧 Advanced Configuration

### Custom Audit Behavior

```csharp
public class CustomAuditInterceptor : AuditInterceptor
{
    public CustomAuditInterceptor(ICurrentUserService currentUserService) 
        : base(currentUserService) { }

    protected override void UpdateAuditFields(DbContext? context)
    {
        base.UpdateAuditFields(context);
        
        // Add custom audit logic here
        foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
            {
                // Custom creation logic
            }
        }
    }
}

// Register custom interceptor
services.AddScoped<AuditInterceptor, CustomAuditInterceptor>();
```

### Multiple DbContexts

```csharp
// Each DbContext can have its own audit interceptor
services.AddDbContext<ProductDbContext>((provider, options) =>
{
    var interceptor = provider.GetRequiredService<AuditInterceptor>();
    options.UseSqlServer(connectionString).AddInterceptors(interceptor);
});

services.AddDbContext<OrderDbContext>((provider, options) =>
{
    var interceptor = provider.GetRequiredService<AuditInterceptor>();
    options.UseNpgsql(connectionString).AddInterceptors(interceptor);
});
```

## 🏢 Enterprise Identity Integration

**Diiwo.Core** powers the enterprise features in **[Diiwo.Identity](https://github.com/diiwo/diiwo-identity)**, providing automatic audit trails for complete identity management solutions.

### Real-World Example: User Management with Audit Trail

```csharp
// Using Diiwo.Identity with Diiwo.Core automatic auditing
public class AppUser : DomainEntity  // ✅ Inherits full audit capabilities
{
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    // ✅ CreatedAt, UpdatedAt, CreatedBy, UpdatedBy automatically managed!
    // ✅ Soft delete preserves user history for compliance
    // ✅ State management tracks user lifecycle
}

// Service layer - zero manual audit code needed!
public class UserService
{
    public async Task<AppUser> CreateUserAsync(string email, string password)
    {
        var user = new AppUser
        {
            Email = email,
            PasswordHash = hashPassword(password)
            // ✅ All audit fields populated automatically by AuditInterceptor!
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();  // Triggers automatic audit population

        return user;  // user.CreatedAt, CreatedBy, etc. are now populated
    }

    public async Task DeactivateUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        user.State = EntityState.Terminated;  // Soft delete

        await _context.SaveChangesAsync();
        // ✅ UpdatedAt and UpdatedBy automatically set!
        // ✅ User preserved for audit/compliance requirements
    }
}
```

### Enterprise Permission System with Audit

```csharp
// Permission entities with full audit trail
public class AppPermission : DomainEntity
{
    public required string Resource { get; set; }
    public required string Action { get; set; }
    // ✅ Every permission change tracked automatically
}

public class AppUserPermission : DomainEntity
{
    public Guid UserId { get; set; }
    public Guid PermissionId { get; set; }
    public bool IsGranted { get; set; }
    // ✅ Full audit trail for permission assignments
}

// Permission assignments automatically audited
await _permissionService.GrantUserPermissionAsync(userId, permissionId);
// ✅ Who granted the permission and when - automatically tracked!
```

### Compliance & Regulatory Requirements

```csharp
// Query audit trail for compliance reports
var userAuditTrail = await _context.Users
    .Where(u => u.Id == userId)
    .Select(u => new AuditReport
    {
        EntityId = u.Id,
        CreatedAt = u.CreatedAt,
        CreatedBy = u.CreatedBy,
        LastUpdatedAt = u.UpdatedAt,
        LastUpdatedBy = u.UpdatedBy,
        CurrentState = u.State,
        IsActive = u.IsActive
    })
    .FirstOrDefaultAsync();

// Soft-deleted entities preserved for audit
var deletedUsers = await _context.Users
    .IgnoreQueryFilters()  // Include soft-deleted
    .Where(u => u.State == EntityState.Terminated)
    .ToListAsync();
// ✅ Full history preserved for regulatory compliance
```

## 🏛️ Integration Examples

### With Clean Architecture

```csharp
// Domain Layer - Pure entities
public class Customer : DomainEntity
{
    public string Name { get; private set; }
    public Email Email { get; private set; }
    
    // Domain methods
    public void UpdateEmail(Email newEmail)
    {
        Email = newEmail;
        // UpdatedAt and UpdatedBy automatically set on save
    }
}

// Infrastructure Layer - DbContext
public class ApplicationDbContext : DbContext
{
    // ... DbContext implementation with AuditInterceptor
}
```

### With MediatR

```csharp
public class CreateProductCommand : IRequest<Guid>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}

public class CreateProductHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly ApplicationDbContext _context;
    
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product 
        {
            Name = request.Name,
            Price = request.Price
            // Audit fields automatically populated
        };
        
        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);
        
        return product.Id;
    }
}
```

### With Repository Pattern

```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetActiveAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task SoftDeleteAsync(Guid id);
}

public class Repository<T> : IRepository<T> where T : AuditableEntity
{
    private readonly DbContext _context;
    
    public async Task<IEnumerable<T>> GetActiveAsync()
    {
        return await _context.Set<T>()
            .Where(x => x.IsActive)
            .ToListAsync();
    }
    
    public async Task SoftDeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            entity.SoftDelete(); // Uses Diiwo.Core functionality
            await _context.SaveChangesAsync();
        }
    }
}
```

## 🌐 Framework Compatibility

| Framework | Support | Notes |
|-----------|---------|--------|
| ASP.NET Core | ✅ Full | Recommended for web applications |
| Console Apps | ✅ Full | Perfect for background services |
| Desktop (WPF/WinUI) | ✅ Full | Great for desktop applications |
| Blazor | ✅ Full | Works with both Server and WebAssembly |
| Worker Services | ✅ Full | Ideal for background processing |
| Azure Functions | ✅ Full | Serverless applications |
| Minimal APIs | ✅ Full | Lightweight web APIs |

## 📊 Performance

- **Zero Runtime Overhead** - Entities are POCOs with no runtime proxies
- **Minimal Memory Footprint** - Only essential properties added to base entities  
- **EF Core Optimized** - Interceptors use EF Core's native change tracking
- **Lazy Evaluation** - Computed properties use lazy evaluation where possible

## 🛠️ Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Setup

```bash
# Clone the repository
git clone https://github.com/diiwo/diiwo-core.git
cd diiwo-core

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run tests
dotnet test
```

## 🔗 Related Projects

- **[Diiwo.Identity](https://github.com/diiwo/diiwo-identity)** - Complete identity management with dual architectures

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

- 📖 **Documentation**: [Getting Started Guide](GETTING-STARTED.md)
- 🏗️ **Architecture**: [Architecture Guide](ARCHITECTURE.md)
- 🐛 **Issues**: [GitHub Issues](https://github.com/diiwo/diiwo-core/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/diiwo/diiwo-core/discussions)

---

<div align="center">

**Built with ❤️ by the [DIIWO Team](https://diiwo.com)**

</div>