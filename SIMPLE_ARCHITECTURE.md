# Simple Architecture Guide - Campus Events Project

## Overview
This project uses a **simple, straightforward architecture** designed for 2nd year students to easily understand and contribute to. We've removed complex patterns and focused on the core SOEN 341 requirements.

## Project Structure

```
Controllers/           # API endpoints (the main logic)
├── AuthController.cs      # Login, signup, logout
├── EventsController.cs    # Event CRUD operations
├── CategoriesController.cs # Category management
├── AnalyticsController.cs  # Event analytics
├── NotificationsController.cs
└── QrScanController.cs

Data/
└── AppDbContext.cs        # Database connection

Models/                   # Database entities
├── User.cs
├── Event.cs
├── Category.cs
└── ... (other models)

Program.cs                # Application startup
```

## How Authentication Works

### Simple Session-Based Auth
- When user logs in, we store their ID and role in the session
- Each controller checks the session to see if user is logged in
- No complex JWT tokens or advanced auth patterns

### Helper Methods (used in all controllers)
```csharp
// Get current user ID from session
private int? GetCurrentUserId()
{
    var userIdString = HttpContext.Session.GetString("UserId");
    if (int.TryParse(userIdString, out var userId))
        return userId;
    return null;
}

// Check if current user is admin
private bool IsAdmin()
{
    var role = HttpContext.Session.GetString("UserRole");
    return role == "Admin";
}
```

## How to Add a New Endpoint

### 1. Add Method to Controller
```csharp
[HttpPost("my-new-endpoint")]
public async Task<IActionResult> MyNewEndpoint([FromBody] MyRequest request)
{
    // Check if user is logged in
    var userId = GetCurrentUserId();
    if (userId == null)
    {
        return Unauthorized(new { message = "Must be logged in" });
    }

    // Your logic here
    var result = await _context.SomeTable.AddAsync(new SomeEntity { ... });
    await _context.SaveChangesAsync();

    return Ok(new { message = "Success", data = result });
}
```

### 2. Create Request Class (at bottom of controller file)
```csharp
public class MyRequest
{
    public required string SomeField { get; set; }
    public int? OptionalField { get; set; }
}
```

## How to Query the Database

### Basic Queries
```csharp
// Get all records
var items = await _context.Items.ToListAsync();

// Get by ID
var item = await _context.Items.FindAsync(id);

// Get with conditions
var items = await _context.Items
    .Where(i => i.IsActive)
    .OrderBy(i => i.Name)
    .ToListAsync();

// Get with related data
var events = await _context.Events
    .Include(e => e.Organizer)
    .Include(e => e.Organization)
    .ToListAsync();
```

### Adding Records
```csharp
var newItem = new Item
{
    Name = request.Name,
    Description = request.Description,
    CreatedAt = DateTime.UtcNow
};

_context.Items.Add(newItem);
await _context.SaveChangesAsync();
```

### Updating Records
```csharp
var item = await _context.Items.FindAsync(id);
if (item == null)
    return NotFound();

item.Name = request.Name ?? item.Name;
item.Description = request.Description ?? item.Description;

await _context.SaveChangesAsync();
```

## Common Patterns Used

### 1. Authentication Check
```csharp
var userId = GetCurrentUserId();
if (userId == null)
    return Unauthorized(new { message = "Must be logged in" });
```

### 2. Admin Check
```csharp
if (!IsAdmin())
    return Forbid("Only administrators can do this");
```

### 3. Resource Ownership Check
```csharp
var userId = GetCurrentUserId();
var userRole = GetCurrentUserRole();

if (userId == null || (item.OwnerId != userId.Value && userRole != "Admin"))
    return Forbid("You can only access your own items");
```

### 4. Basic Validation
```csharp
if (string.IsNullOrEmpty(request.Name))
    return BadRequest(new { message = "Name is required" });

if (request.Capacity <= 0)
    return BadRequest(new { message = "Capacity must be greater than 0" });
```

### 5. Simple Response Mapping
```csharp
private static object MapToDto(Entity entity) => new
{
    entity.Id,
    entity.Name,
    entity.Description,
    CreatedAt = entity.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
};
```

## User Roles

- **Student**: Can browse events, claim tickets, save events
- **Organizer**: Can create/edit events, view analytics, scan tickets
- **Admin**: Can do everything + approve organizers, moderate events

## Core Features (SOEN 341 Requirements)

### Student Features
- Browse and search events with filters
- Save events to personal calendar
- Claim tickets (free or paid)
- Receive QR codes for tickets

### Organizer Features
- Create and manage events
- View analytics dashboard
- Export attendee lists
- QR scanner for ticket validation

### Admin Features
- Approve organizer accounts
- Moderate event listings
- View platform analytics
- Manage organizations

## What We Removed (Advanced Patterns)

- ❌ Services layer (AuthService, ValidationService, AuditService)
- ❌ DTOs folder (using simple anonymous objects)
- ❌ Global exception middleware
- ❌ Complex validation service
- ❌ Audit logging
- ❌ Advanced authorization patterns

## Benefits of This Simple Approach

1. **Easy to Understand**: Direct controller logic without abstraction
2. **Fast to Modify**: Add features without learning complex patterns
3. **Less Code**: Fewer files and concepts to manage
4. **Focus on Core**: Only what's needed for SOEN 341
5. **Better for Learning**: Understand basics before advanced patterns

## Tips for Team Members

1. **Start with existing controllers** - copy patterns you see
2. **Use the helper methods** - GetCurrentUserId(), IsAdmin(), etc.
3. **Follow the same structure** - authentication check, validation, database operation, response
4. **Ask questions** - this is designed to be simple and understandable
5. **Test your endpoints** - use Postman or the frontend to test

## Database Models

The database models in the `Models/` folder define the structure:
- **User**: Students, organizers, admins
- **Event**: Event information
- **Ticket**: QR-coded tickets
- **Category**: Event categories
- **Organization**: Campus organizations
- **EventAnalytics**: Performance metrics

Each model has properties and navigation properties for relationships.

## Questions?

If you're confused about anything:
1. Look at existing controllers for examples
2. Check this documentation
3. Ask your teammates
4. The code is designed to be self-explanatory with comments

Remember: **Simple is better than complex** for this project!
