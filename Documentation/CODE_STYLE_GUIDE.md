# Campus Events System - Code Style Guide

## Overview

This document defines the coding standards, conventions, and best practices for the Campus Events system. Following these guidelines ensures code consistency, maintainability, and readability across the entire project.

## Table of Contents

1. [General Principles](#general-principles)
2. [Naming Conventions](#naming-conventions)
3. [Code Organization](#code-organization)
4. [Documentation Standards](#documentation-standards)
5. [Error Handling](#error-handling)
6. [Async/Await Patterns](#asyncawait-patterns)
7. [Database Patterns](#database-patterns)
8. [Security Guidelines](#security-guidelines)
9. [Testing Standards](#testing-standards)

---

## General Principles

### Code Quality

- **Readability First**: Code should be self-documenting and easy to understand
- **DRY (Don't Repeat Yourself)**: Avoid code duplication; extract common logic
- **SOLID Principles**: Follow Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion
- **KISS (Keep It Simple, Stupid)**: Prefer simple solutions over complex ones
- **YAGNI (You Aren't Gonna Need It)**: Don't add functionality until it's needed

### Code Formatting

- Use 4 spaces for indentation (not tabs)
- Use consistent brace style (opening brace on same line)
- Maximum line length: 120 characters
- Remove trailing whitespace
- Use meaningful variable names
- Avoid magic numbers (use constants)

---

## Naming Conventions

### Classes and Interfaces

- **PascalCase** for class names: `EventService`, `UserController`
- **PascalCase** for interface names: `IEventService` (prefix with 'I')
- Use descriptive, noun-based names: `TicketSigningService`, not `TicketService`
- Avoid abbreviations unless widely understood: `AppDbContext` (App is acceptable)

### Methods

- **PascalCase** for method names: `CreateEventAsync`, `ValidateEmail`
- Use verb-based names: `GetUserById`, `CalculateDistance`, `ValidateInput`
- Async methods end with `Async`: `GetUserAsync`, `SaveChangesAsync`
- Boolean methods use `Is`, `Has`, `Can`, `Should`: `IsValidEmail`, `HasPermission`, `CanEdit`

### Variables and Parameters

- **camelCase** for local variables: `userId`, `eventTitle`, `isValid`
- **camelCase** for parameters: `userId`, `eventId`, `email`
- Use descriptive names: `userEmail` not `email`, `eventDate` not `date`
- Avoid single-letter variables except in loops: `i`, `j`, `k` for loop counters

### Constants

- **PascalCase** for constants: `MaxPasswordLength`, `DefaultPageSize`
- Group related constants in static classes: `Constants.Validation.MaxPasswordLength`
- Use `const` for compile-time constants, `static readonly` for runtime constants

### Private Fields

- **camelCase** with underscore prefix: `_context`, `_logger`, `_service`
- Use underscore prefix to distinguish from parameters: `_dbContext` vs `dbContext` parameter

### Properties

- **PascalCase** for public properties: `UserId`, `EventTitle`, `IsActive`
- Use auto-properties when possible: `public int Id { get; set; }`
- Use expression-bodied properties for simple getters: `public string FullName => $"{FirstName} {LastName}";`

### Enums

- **PascalCase** for enum names: `EventCategory`, `UserRole`, `ApprovalStatus`
- **PascalCase** for enum values: `EventCategory.Workshop`, `UserRole.Admin`
- Use singular names for enums: `EventCategory` not `EventCategories`

---

## Code Organization

### File Structure

```
namespace CampusEvents.Services;

/// <summary>
/// Class documentation
/// </summary>
public class ServiceName
{
    // Constants
    private const int MaxRetries = 3;
    
    // Private fields
    private readonly AppDbContext _context;
    
    // Constructor
    public ServiceName(AppDbContext context)
    {
        _context = context;
    }
    
    // Public methods
    public async Task<Result> PublicMethodAsync()
    {
        // Implementation
    }
    
    // Private methods
    private void PrivateMethod()
    {
        // Implementation
    }
}
```

### Using Statements

- Group using statements:
  1. System namespaces
  2. Third-party libraries
  3. Application namespaces
- Sort alphabetically within each group
- Remove unused using statements

### Namespace Organization

- One class per file
- File name matches class name
- Namespace matches folder structure: `CampusEvents.Services` for `Services/` folder

---

## Documentation Standards

### XML Documentation Comments

All public classes, methods, properties, and parameters must have XML documentation comments:

```csharp
/// <summary>
/// Creates a new event in the system.
/// </summary>
/// <param name="eventData">Event data containing title, description, date, etc.</param>
/// <returns>Created event with assigned ID</returns>
/// <exception cref="ArgumentException">Thrown when event data is invalid</exception>
public async Task<Event> CreateEventAsync(EventData eventData)
{
    // Implementation
}
```

### Inline Comments

- Use inline comments to explain **why**, not **what**
- Avoid obvious comments: `// Increment counter` (obvious)
- Use comments for complex logic, business rules, or non-obvious decisions
- Keep comments up-to-date with code changes

### Code Examples

Include code examples in XML documentation for complex methods:

```csharp
/// <example>
/// <code>
/// var result = await service.CreateEventAsync(new EventData
/// {
///     Title = "Workshop",
///     Date = DateTime.UtcNow.AddDays(7)
/// });
/// </code>
/// </example>
```

---

## Error Handling

### Exception Handling

- Use specific exception types: `ArgumentException`, `InvalidOperationException`
- Create custom exceptions for domain-specific errors
- Always log exceptions with context
- Never swallow exceptions silently
- Use `ErrorHandler` utility for consistent error handling

### Validation

- Validate input at method boundaries
- Return early for invalid input
- Use `ValidationHelper` for common validations
- Provide clear, user-friendly error messages

### Error Messages

- User-facing messages: Friendly, actionable, no technical jargon
- Log messages: Technical, include context, stack traces
- Never expose sensitive information in error messages

---

## Async/Await Patterns

### Async Method Naming

- All async methods end with `Async`: `GetUserAsync`, `SaveEventAsync`
- Use `async Task<T>` for methods returning values
- Use `async Task` for methods not returning values

### Async Best Practices

- Always await async calls: `await GetUserAsync()`, not `GetUserAsync()`
- Use `ConfigureAwait(false)` in library code (not needed in ASP.NET Core)
- Avoid `Task.Result` or `.Wait()` (causes deadlocks)
- Use `Task.WhenAll` for parallel operations

### Example

```csharp
public async Task<Event> GetEventAsync(int eventId)
{
    var event = await _context.Events
        .Include(e => e.Organizer)
        .FirstOrDefaultAsync(e => e.Id == eventId);
    
    if (event == null)
        throw new NotFoundException($"Event {eventId} not found");
    
    return event;
}
```

---

## Database Patterns

### Entity Framework Core

- Use async methods: `ToListAsync()`, `FirstOrDefaultAsync()`, `SaveChangesAsync()`
- Use `Include()` for eager loading related entities
- Use projections to limit data: `.Select(e => new { e.Id, e.Title })`
- Avoid N+1 queries (use `Include()` or projections)

### Query Patterns

- Use LINQ for queries
- Use `QueryHelper` extensions for common query patterns
- Filter at database level, not in memory
- Use pagination for large datasets

### Transactions

- Use `BeginTransactionAsync()` for multi-step operations
- Always commit or rollback transactions
- Use `using` statement for transaction management

---

## Security Guidelines

### Password Handling

- Never store plain text passwords
- Use `PasswordHelper.HashPassword()` for hashing
- Use `PasswordHelper.VerifyPassword()` for verification
- Validate password strength before hashing

### Data Encryption

- Use `EncryptionService` for sensitive data (license numbers, plates)
- Never log sensitive information
- Use secure key management

### Input Validation

- Always validate and sanitize user input
- Use `ValidationHelper` for common validations
- Sanitize input before display: `ValidationHelper.SanitizeInput()`
- Use parameterized queries (EF Core handles this)

### Authentication/Authorization

- Check user authentication before operations
- Verify user permissions/roles
- Use role-based authorization attributes
- Never trust client-side validation alone

---

## Testing Standards

### Unit Tests

- Test one thing per test method
- Use descriptive test names: `GetUserAsync_WithValidId_ReturnsUser`
- Arrange-Act-Assert pattern
- Mock dependencies
- Test edge cases and error conditions

### Integration Tests

- Test real database operations
- Clean up test data after tests
- Use test database, not production
- Test complete workflows

### Test Organization

- One test class per class being tested
- Test class name: `{ClassName}Tests`
- Group related tests with regions or nested classes

---

## Code Review Checklist

### Before Submitting

- [ ] Code follows naming conventions
- [ ] All public members have XML documentation
- [ ] Code is properly formatted
- [ ] No compiler warnings
- [ ] All tests pass
- [ ] Error handling is appropriate
- [ ] Security considerations addressed
- [ ] No hardcoded values (use constants)
- [ ] No commented-out code
- [ ] No unused code (methods, variables, using statements)

### During Review

- [ ] Code is readable and maintainable
- [ ] Logic is correct
- [ ] Performance considerations addressed
- [ ] Security vulnerabilities checked
- [ ] Tests are comprehensive
- [ ] Documentation is accurate

---

## Tools and Resources

### IDE Settings

- Enable XML documentation warnings
- Configure code formatting rules
- Use code analyzers (Roslyn analyzers)
- Enable nullable reference types

### Code Analysis

- Use `dotnet format` for code formatting
- Use `dotnet analyze` for code analysis
- Fix all warnings before committing

### Documentation

- Use XML documentation comments
- Keep documentation up-to-date
- Include code examples for complex APIs
- Document assumptions and constraints

---

**Document Version**: 1.0
**Last Updated**: 2025-01-XX
**Maintained By**: Development Team

