# Campus Events System - Developer Guide

## Overview

This guide provides comprehensive information for developers working on the Campus Events system, including coding standards, best practices, and development workflows.

## Table of Contents

1. [Getting Started](#getting-started)
2. [Project Structure](#project-structure)
3. [Coding Standards](#coding-standards)
4. [Development Workflow](#development-workflow)
5. [Testing Guidelines](#testing-guidelines)
6. [Common Tasks](#common-tasks)
7. [Troubleshooting](#troubleshooting)

---

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- Git
- IDE (Visual Studio, VS Code, or Rider)
- Entity Framework Core Tools

### Initial Setup

1. **Clone Repository**
   ```bash
   git clone https://github.com/your-org/MONDAY_FK-X-SOEN341_PROJECT_F25.git
   cd MONDAY_FK-X-SOEN341_PROJECT_F25
   ```

2. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure Database**
   - SQLite is used by default (no configuration needed)
   - For SQL Server, update `appsettings.Development.json`

4. **Run Migrations**
   ```bash
   dotnet ef database update
   ```

5. **Run Application**
   ```bash
   dotnet watch run
   ```

---

## Project Structure

### Directory Organization

```
CampusEvents/
├── Data/                 # Database context and data access
│   ├── AppDbContext.cs   # EF Core database context
│   └── DbCSVCommunicator.cs  # CSV import/export
├── Models/               # Domain entities
│   ├── User.cs
│   ├── Event.cs
│   ├── Ticket.cs
│   └── ...
├── Services/             # Business logic services
│   ├── CarpoolService.cs
│   ├── RoomRentalService.cs
│   ├── TicketSigningService.cs
│   └── ...
├── Pages/                # Razor Pages (UI)
│   ├── Student/          # Student pages
│   ├── Organizer/        # Organizer pages
│   ├── Admin/            # Admin pages
│   └── Shared/           # Shared components
├── Migrations/           # EF Core migrations
├── wwwroot/              # Static files (CSS, JS, images)
├── Documentation/        # Project documentation
└── Program.cs            # Application entry point
```

### Key Files

- **Program.cs**: Service registration and middleware configuration
- **AppDbContext.cs**: Database context and entity configuration
- **Models/**: Domain entities representing business concepts
- **Services/**: Business logic and reusable functionality
- **Pages/**: Razor Pages for UI

---

## Coding Standards

### Naming Conventions

#### Classes and Methods
- **Classes**: PascalCase (e.g., `CarpoolService`)
- **Methods**: PascalCase (e.g., `CreateOfferAsync`)
- **Parameters**: camelCase (e.g., `userId`, `eventId`)

#### Variables and Fields
- **Public Properties**: PascalCase (e.g., `UserId`)
- **Private Fields**: camelCase with underscore prefix (e.g., `_context`)
- **Local Variables**: camelCase (e.g., `var eventList`)

#### Constants
- **Constants**: PascalCase (e.g., `DefaultTimeoutMinutes`)
- **Static Readonly**: PascalCase (e.g., `EarthRadiusKm`)

### Code Organization

#### File Structure
```csharp
// 1. Using statements
using System;
using CampusEvents.Models;

// 2. Namespace
namespace CampusEvents.Services;

// 3. Class documentation
/// <summary>
/// Service description
/// </summary>
public class ServiceName
{
    // 4. Private fields
    private readonly AppDbContext _context;
    
    // 5. Constructor
    public ServiceName(AppDbContext context)
    {
        _context = context;
    }
    
    // 6. Public methods
    public async Task<Result> MethodAsync() { }
    
    // 7. Private helper methods
    private void HelperMethod() { }
}
```

### Documentation Standards

#### XML Documentation Comments

All public classes, methods, and properties should have XML documentation:

```csharp
/// <summary>
/// Brief description of what the method does.
/// </summary>
/// <param name="userId">Description of parameter</param>
/// <returns>Description of return value</returns>
/// <remarks>
/// Additional details, examples, or important notes.
/// </remarks>
public async Task<User> GetUserAsync(int userId)
{
    // Implementation
}
```

#### Inline Comments

Use inline comments to explain:
- Complex business logic
- Non-obvious code decisions
- Workarounds or temporary solutions
- Algorithm explanations

```csharp
// Calculate distance using Haversine formula
// This formula calculates great-circle distance between two points
var distance = CalculateDistance(lat1, lon1, lat2, lon2);
```

### Async/Await Patterns

#### Always Use Async for I/O Operations

```csharp
// ✅ Good
public async Task<List<Event>> GetEventsAsync()
{
    return await _context.Events.ToListAsync();
}

// ❌ Bad
public List<Event> GetEvents()
{
    return _context.Events.ToList(); // Synchronous I/O
}
```

#### ConfigureAwait(false) for Library Code

```csharp
// For library code (services)
return await _context.Events.ToListAsync().ConfigureAwait(false);
```

### Error Handling

#### Result Tuple Pattern

Services should return tuples for operation results:

```csharp
public async Task<(bool Success, string Message, CarpoolOffer? Offer)> 
    CreateOfferAsync(...)
{
    // Validation
    if (driver.Status != DriverStatus.Active)
        return (false, "Driver not active", null);
    
    // Success
    return (true, "Offer created successfully", offer);
}
```

#### Exception Handling

Catch specific exceptions when possible:

```csharp
try
{
    await _context.SaveChangesAsync();
}
catch (DbUpdateException ex)
{
    // Handle database update errors
    return (false, "Database update failed", null);
}
catch (Exception ex)
{
    // Log and handle unexpected errors
    ErrorHandler.LogError(ex, "CreateOfferAsync");
    return (false, "An unexpected error occurred", null);
}
```

### Dependency Injection

#### Constructor Injection

Always use constructor injection for dependencies:

```csharp
public class CarpoolService
{
    private readonly AppDbContext _context;
    private readonly NotificationService _notificationService;
    
    public CarpoolService(
        AppDbContext context,
        NotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }
}
```

#### Service Registration

Register services in `Program.cs` with appropriate lifetimes:

```csharp
// Singleton: Stateless, thread-safe services
builder.Services.AddSingleton<TicketSigningService>();

// Scoped: Per-request services (most common)
builder.Services.AddScoped<CarpoolService>();

// Transient: Lightweight, no shared state
builder.Services.AddTransient<DbCSVCommunicator>();
```

---

## Development Workflow

### Branch Strategy

- **main**: Production-ready code
- **develop**: Integration branch for features
- **feature/**: Feature branches (e.g., `feature/add-notifications`)
- **bugfix/**: Bug fix branches (e.g., `bugfix/fix-ticket-validation`)

### Commit Messages

Use conventional commit format:

```
type(scope): subject

body (optional)

footer (optional)
```

**Types**:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation
- `style`: Code style changes
- `refactor`: Code refactoring
- `test`: Test additions/changes
- `chore`: Maintenance tasks

**Examples**:
```
feat(carpool): Add proximity-based offer matching

docs: Update API documentation

fix(tickets): Resolve QR code validation issue
```

### Pull Request Process

1. **Create Feature Branch**
   ```bash
   git checkout -b feature/your-feature
   ```

2. **Make Changes and Commit**
   ```bash
   git add .
   git commit -m "feat: Add new feature"
   ```

3. **Push and Create PR**
   ```bash
   git push origin feature/your-feature
   ```

4. **Code Review**
   - Address review comments
   - Update PR as needed

5. **Merge**
   - Squash and merge to `develop`
   - Delete feature branch

### Database Migrations

#### Creating Migrations

```bash
# Create migration
dotnet ef migrations add MigrationName

# Review generated migration file
# Apply migration
dotnet ef database update
```

#### Migration Best Practices

- **Name migrations descriptively**: `AddNotificationSystem`, `UpdateEventModel`
- **Review generated SQL**: Check for unintended changes
- **Test migrations**: Test both up and down migrations
- **Backup database**: Before applying migrations in production

---

## Testing Guidelines

### Unit Testing

#### Test Structure

```csharp
public class CarpoolServiceTests
{
    private readonly Mock<AppDbContext> _mockContext;
    private readonly CarpoolService _service;
    
    public CarpoolServiceTests()
    {
        _mockContext = new Mock<AppDbContext>();
        _service = new CarpoolService(_mockContext.Object);
    }
    
    [Fact]
    public async Task CreateOfferAsync_ValidInput_ReturnsSuccess()
    {
        // Arrange
        // Act
        // Assert
    }
}
```

#### Test Naming

Use descriptive test names:

```csharp
[Fact]
public async Task CreateOfferAsync_DriverNotActive_ReturnsFailure()
{
    // Test implementation
}
```

### Integration Testing

#### Using In-Memory Database

```csharp
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase(databaseName: "TestDb")
    .Options;

using var context = new AppDbContext(options);
var service = new CarpoolService(context);

// Test implementation
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "FullyQualifiedName~CarpoolServiceTests"
```

---

## Common Tasks

### Adding a New Service

1. **Create Service Class**
   ```csharp
   namespace CampusEvents.Services;
   
   public class NewService
   {
       private readonly AppDbContext _context;
       
       public NewService(AppDbContext context)
       {
           _context = context;
       }
       
       // Service methods
   }
   ```

2. **Register in Program.cs**
   ```csharp
   builder.Services.AddScoped<NewService>();
   ```

3. **Add XML Documentation**
   ```csharp
   /// <summary>
   /// Service description
   /// </summary>
   public class NewService
   ```

### Adding a New Model

1. **Create Model Class**
   ```csharp
   namespace CampusEvents.Models;
   
   public class NewModel
   {
       public int Id { get; set; }
       // Properties
   }
   ```

2. **Add to AppDbContext**
   ```csharp
   public DbSet<NewModel> NewModels { get; set; }
   ```

3. **Create Migration**
   ```bash
   dotnet ef migrations add AddNewModel
   dotnet ef database update
   ```

### Adding a New Page

1. **Create Page Files**
   - `Pages/NewPage.cshtml` (view)
   - `Pages/NewPage.cshtml.cs` (page model)

2. **Implement Page Model**
   ```csharp
   public class NewPageModel : PageModel
   {
       public async Task OnGetAsync()
       {
           // Page logic
       }
   }
   ```

3. **Add Route** (if custom route needed)
   ```csharp
   [Route("/custom-route")]
   public class NewPageModel : PageModel
   ```

### Adding Validation

1. **Use ValidationHelper**
   ```csharp
   if (!ValidationHelper.IsValidEmail(email))
       return (false, "Invalid email format", null);
   ```

2. **Add Model Validation Attributes**
   ```csharp
   [Required]
   [StringLength(200)]
   public string Title { get; set; }
   ```

---

## Troubleshooting

### Common Issues

#### Database Connection Errors

**Problem**: Cannot connect to database

**Solutions**:
- Verify connection string in `appsettings.json`
- Check database server is running
- Verify user permissions
- Check firewall rules

#### Migration Errors

**Problem**: Migration fails

**Solutions**:
- Review migration file for errors
- Check database state matches migration history
- Try dropping and recreating database (dev only)

#### Service Not Found

**Problem**: Dependency injection fails

**Solutions**:
- Verify service is registered in `Program.cs`
- Check service lifetime matches usage
- Ensure constructor parameters match registered services

#### Build Errors

**Problem**: Project won't build

**Solutions**:
- Run `dotnet restore`
- Check for missing using statements
- Verify all dependencies are installed
- Clean and rebuild: `dotnet clean && dotnet build`

---

## Best Practices

### Performance

1. **Use Async/Await**: All I/O operations should be async
2. **Eager Loading**: Use `.Include()` to avoid N+1 queries
3. **Pagination**: Paginate large result sets
4. **Caching**: Cache frequently accessed, rarely changed data

### Security

1. **Input Validation**: Always validate user input
2. **SQL Injection**: Use parameterized queries (EF Core does this)
3. **XSS Prevention**: HTML-encode user input in views
4. **Authorization**: Check permissions before operations
5. **Sensitive Data**: Encrypt sensitive data at rest

### Code Quality

1. **DRY Principle**: Don't Repeat Yourself
2. **SOLID Principles**: Follow SOLID design principles
3. **Single Responsibility**: Each class should have one responsibility
4. **Code Reviews**: Always review code before merging
5. **Documentation**: Document public APIs and complex logic

---

## Resources

### Documentation

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [Razor Pages](https://docs.microsoft.com/aspnet/core/razor-pages)

### Tools

- [.NET CLI](https://docs.microsoft.com/dotnet/core/tools/)
- [Entity Framework Tools](https://docs.microsoft.com/ef/core/cli/dotnet)
- [Visual Studio Code](https://code.visualstudio.com/)

---

**Document Version**: 1.0
**Last Updated**: 2025-01-XX
**Maintained By**: Development Team

