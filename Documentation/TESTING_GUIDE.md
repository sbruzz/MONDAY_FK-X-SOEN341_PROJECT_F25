# Campus Events System - Testing Guide

## Overview

This guide provides comprehensive information about testing strategies, test implementation, and best practices for the Campus Events system.

## Table of Contents

1. [Testing Strategy](#testing-strategy)
2. [Unit Testing](#unit-testing)
3. [Integration Testing](#integration-testing)
4. [Test Organization](#test-organization)
5. [Test Data Management](#test-data-management)
6. [Running Tests](#running-tests)
7. [Best Practices](#best-practices)

---

## Testing Strategy

### Testing Pyramid

```
        /\
       /  \
      / E2E \
     /--------\
    / Integration \
   /--------------\
  /   Unit Tests   \
 /------------------\
```

- **Unit Tests**: Fast, isolated, test individual components
- **Integration Tests**: Test component interactions
- **E2E Tests**: Test complete user workflows (optional)

### Test Coverage Goals

- **Unit Tests**: 80%+ code coverage
- **Integration Tests**: Critical paths covered
- **Service Layer**: 100% coverage
- **Helper Classes**: 100% coverage

---

## Unit Testing

### Test Structure

#### Basic Test Class

```csharp
using Xunit;
using Moq;
using CampusEvents.Services;
using CampusEvents.Data;
using CampusEvents.Models;

namespace CampusEvents.Tests.UnitTests;

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
        var driverId = 1;
        var eventId = 1;
        // ... setup mocks
        
        // Act
        var (success, message, offer) = await _service.CreateOfferAsync(...);
        
        // Assert
        Assert.True(success);
        Assert.NotNull(offer);
        Assert.Equal("Offer created successfully", message);
    }
}
```

### Testing Services

#### Mocking Dependencies

```csharp
// Mock DbContext
var mockContext = new Mock<AppDbContext>();
var mockSet = new Mock<DbSet<Driver>>();

mockContext.Setup(c => c.Drivers).Returns(mockSet.Object);

// Mock async operations
mockSet.Setup(m => m.FindAsync(It.IsAny<int>()))
    .ReturnsAsync(new Driver { Id = 1, Status = DriverStatus.Active });
```

#### Testing Result Tuples

```csharp
[Fact]
public async Task RegisterDriverAsync_InvalidCapacity_ReturnsFailure()
{
    // Arrange
    var invalidCapacity = 100; // Exceeds max
    
    // Act
    var (success, message, driver) = await _service.RegisterDriverAsync(
        userId: 1,
        capacity: invalidCapacity,
        vehicleType: VehicleType.Sedan,
        driverType: DriverType.Student
    );
    
    // Assert
    Assert.False(success);
    Assert.Null(driver);
    Assert.Contains("capacity", message.ToLower());
}
```

### Testing Helpers

#### Static Helper Classes

```csharp
[Fact]
public void IsValidEmail_ValidEmail_ReturnsTrue()
{
    // Arrange
    var email = "user@example.com";
    
    // Act
    var result = ValidationHelper.IsValidEmail(email);
    
    // Assert
    Assert.True(result);
}

[Theory]
[InlineData("invalid")]
[InlineData("@example.com")]
[InlineData("user@")]
[InlineData("")]
[InlineData(null)]
public void IsValidEmail_InvalidEmail_ReturnsFalse(string? email)
{
    // Act
    var result = ValidationHelper.IsValidEmail(email);
    
    // Assert
    Assert.False(result);
}
```

---

## Integration Testing

### In-Memory Database

#### Setup

```csharp
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Services;

public class CarpoolServiceIntegrationTests
{
    private AppDbContext _context;
    private CarpoolService _service;
    
    public CarpoolServiceIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        
        _context = new AppDbContext(options);
        _service = new CarpoolService(_context);
    }
    
    [Fact]
    public async Task CreateOfferAsync_ValidInput_CreatesOfferInDatabase()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@test.com", ... };
        var driver = new Driver { Id = 1, UserId = 1, Status = DriverStatus.Active, ... };
        var event = new Event { Id = 1, ... };
        
        _context.Users.Add(user);
        _context.Drivers.Add(driver);
        _context.Events.Add(event);
        await _context.SaveChangesAsync();
        
        // Act
        var (success, message, offer) = await _service.CreateOfferAsync(
            driverId: 1,
            eventId: 1,
            departureInfo: "Test departure",
            departureTime: DateTime.UtcNow.AddDays(1)
        );
        
        // Assert
        Assert.True(success);
        Assert.NotNull(offer);
        
        var savedOffer = await _context.CarpoolOffers.FindAsync(offer.Id);
        Assert.NotNull(savedOffer);
        Assert.Equal("Test departure", savedOffer.DepartureInfo);
    }
    
    public void Dispose()
    {
        _context?.Dispose();
    }
}
```

### Test Data Setup

#### Test Helpers

```csharp
public static class TestDataHelper
{
    public static User CreateTestUser(int id = 1, UserRole role = UserRole.Student)
    {
        return new User
        {
            Id = id,
            Email = $"user{id}@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
            Name = $"Test User {id}",
            Role = role,
            ApprovalStatus = ApprovalStatus.Approved,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    public static Event CreateTestEvent(int id = 1, int organizerId = 1)
    {
        return new Event
        {
            Id = id,
            Title = $"Test Event {id}",
            Description = "Test description",
            EventDate = DateTime.UtcNow.AddDays(7),
            Location = "Test Location",
            Capacity = 100,
            TicketType = TicketType.Free,
            Price = 0,
            Category = EventCategory.Social,
            OrganizerId = organizerId,
            ApprovalStatus = ApprovalStatus.Approved,
            CreatedAt = DateTime.UtcNow
        };
    }
}
```

---

## Test Organization

### Project Structure

```
CampusEvents.Tests/
├── UnitTests/
│   ├── Services/
│   │   ├── CarpoolServiceTests.cs
│   │   ├── RoomRentalServiceTests.cs
│   │   └── NotificationServiceTests.cs
│   ├── Helpers/
│   │   ├── ValidationHelperTests.cs
│   │   └── FormatHelperTests.cs
│   └── Models/
│       └── ModelValidationTests.cs
├── IntegrationTests/
│   ├── Services/
│   │   ├── CarpoolServiceIntegrationTests.cs
│   │   └── RoomRentalServiceIntegrationTests.cs
│   └── Database/
│       └── DbContextTests.cs
└── TestHelpers/
    ├── TestDataHelper.cs
    └── DbContextFactory.cs
```

### Naming Conventions

#### Test Method Names

Format: `MethodName_Scenario_ExpectedBehavior`

```csharp
[Fact]
public async Task CreateOfferAsync_ValidInput_ReturnsSuccess()

[Fact]
public async Task CreateOfferAsync_DriverNotActive_ReturnsFailure()

[Fact]
public async Task CreateOfferAsync_DuplicateOffer_ReturnsFailure()
```

#### Test Class Names

Format: `ClassNameTests` or `ClassNameIntegrationTests`

```csharp
public class CarpoolServiceTests
public class CarpoolServiceIntegrationTests
```

---

## Test Data Management

### Test Fixtures

```csharp
public class CarpoolServiceFixture : IDisposable
{
    public AppDbContext Context { get; }
    public CarpoolService Service { get; }
    
    public CarpoolServiceFixture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        
        Context = new AppDbContext(options);
        Service = new CarpoolService(Context);
        
        SeedTestData();
    }
    
    private void SeedTestData()
    {
        // Add test data
        Context.Users.Add(TestDataHelper.CreateTestUser());
        Context.SaveChanges();
    }
    
    public void Dispose()
    {
        Context?.Dispose();
    }
}
```

### Test Isolation

#### Unique Database Names

```csharp
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
    .Options;
```

#### Cleanup After Tests

```csharp
public class CarpoolServiceTests : IDisposable
{
    private AppDbContext _context;
    
    public CarpoolServiceTests()
    {
        // Setup
    }
    
    [Fact]
    public async Task TestMethod()
    {
        // Test implementation
    }
    
    public void Dispose()
    {
        _context?.Database.EnsureDeleted();
        _context?.Dispose();
    }
}
```

---

## Running Tests

### Command Line

```bash
# Run all tests
dotnet test

# Run with verbose output
dotnet test --verbosity normal

# Run specific test class
dotnet test --filter "FullyQualifiedName~CarpoolServiceTests"

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run in parallel
dotnet test --parallel
```

### Visual Studio

- **Test Explorer**: View and run tests
- **Code Coverage**: View coverage results
- **Live Unit Testing**: Run tests automatically

### Continuous Integration

#### GitHub Actions Example

```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - name: Setup .NET
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: '9.0.x'
      - name: Restore dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Test
        run: dotnet test --no-build --verbosity normal
```

---

## Best Practices

### 1. Test Independence

```csharp
// ✅ Good: Each test is independent
[Fact]
public async Task Test1() { /* No dependencies on other tests */ }

[Fact]
public async Task Test2() { /* No dependencies on other tests */ }

// ❌ Bad: Tests depend on each other
[Fact]
public async Task Test1() { /* Creates data */ }

[Fact]
public async Task Test2() { /* Depends on Test1's data */ }
```

### 2. Arrange-Act-Assert Pattern

```csharp
[Fact]
public async Task CreateOfferAsync_ValidInput_ReturnsSuccess()
{
    // Arrange: Set up test data and mocks
    var driver = new Driver { ... };
    var event = new Event { ... };
    
    // Act: Execute the method being tested
    var (success, message, offer) = await _service.CreateOfferAsync(...);
    
    // Assert: Verify the results
    Assert.True(success);
    Assert.NotNull(offer);
}
```

### 3. Test One Thing

```csharp
// ✅ Good: Tests one specific behavior
[Fact]
public async Task RegisterDriverAsync_InvalidCapacity_ReturnsFailure()

// ❌ Bad: Tests multiple behaviors
[Fact]
public async Task RegisterDriverAsync_ValidatesCapacityAndEmailAndPhone()
```

### 4. Use Descriptive Names

```csharp
// ✅ Good: Clear and descriptive
[Fact]
public async Task CreateOfferAsync_DriverNotActive_ReturnsFailureWithErrorMessage()

// ❌ Bad: Vague
[Fact]
public async Task TestCreateOffer()
```

### 5. Test Edge Cases

```csharp
[Theory]
[InlineData(0)]      // Minimum
[InlineData(1)]      // Just above minimum
[InlineData(50)]     // Maximum
[InlineData(51)]     // Just above maximum
[InlineData(-1)]     // Negative
public void ValidateCapacity_EdgeCases_ReturnsExpected(int capacity)
{
    // Test implementation
}
```

### 6. Mock External Dependencies

```csharp
// ✅ Good: Mock external services
var mockNotificationService = new Mock<NotificationService>();
mockNotificationService.Setup(s => s.CreateNotificationAsync(...))
    .ReturnsAsync(new Notification());

// ❌ Bad: Use real external services in unit tests
var notificationService = new NotificationService(_context);
```

### 7. Test Error Handling

```csharp
[Fact]
public async Task CreateOfferAsync_DatabaseException_HandlesGracefully()
{
    // Arrange
    _mockContext.Setup(c => c.SaveChangesAsync())
        .ThrowsAsync(new DbUpdateException("Database error"));
    
    // Act & Assert
    await Assert.ThrowsAsync<DbUpdateException>(async () =>
    {
        await _service.CreateOfferAsync(...);
    });
}
```

---

## Test Coverage

### Coverage Goals

- **Services**: 90%+ coverage
- **Helpers**: 100% coverage
- **Models**: Validation logic covered
- **Critical Paths**: 100% coverage

### Coverage Tools

```bash
# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"

# View coverage (requires ReportGenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:coverage.cobertura.xml -targetdir:coverage
```

---

## Common Testing Patterns

### Testing Async Methods

```csharp
[Fact]
public async Task AsyncMethod_Scenario_ExpectedBehavior()
{
    // Use async/await
    var result = await _service.MethodAsync();
    
    // Assert
    Assert.NotNull(result);
}
```

### Testing Exceptions

```csharp
[Fact]
public async Task Method_InvalidInput_ThrowsException()
{
    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(async () =>
    {
        await _service.MethodAsync(invalidInput);
    });
}
```

### Testing Collections

```csharp
[Fact]
public async Task GetEventsAsync_ReturnsOrderedList()
{
    // Act
    var events = await _service.GetEventsAsync();
    
    // Assert
    Assert.NotEmpty(events);
    Assert.True(events.IsOrderedBy(e => e.EventDate));
}
```

---

## Troubleshooting

### Common Issues

#### 1. Tests Not Running

**Solution**: Check test project references and restore packages

#### 2. In-Memory Database Issues

**Solution**: Use unique database names for each test

#### 3. Async Test Failures

**Solution**: Ensure tests are marked as `async Task`, not `async void`

#### 4. Mock Setup Issues

**Solution**: Verify mock setup matches actual method signatures

---

**Document Version**: 1.0
**Last Updated**: 2025-01-XX
**Maintained By**: Development Team

