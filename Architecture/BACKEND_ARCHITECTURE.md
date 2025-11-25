# Backend System Architecture
**Campus Events & Ticketing System**
**SOEN 341 Fall 2025 - Team MONDAY_FK**
**Sprint 3 Architecture Documentation**

---

## Table of Contents
1. [Backend Architecture Overview](#backend-architecture-overview)
2. [High-Level Backend Architecture](#high-level-backend-architecture)
3. [Backend Component Breakdown](#backend-component-breakdown)
4. [Data Flow Diagrams](#data-flow-diagrams)
5. [Backend Layer Details](#backend-layer-details)
6. [Service Components](#service-components)
7. [Database Architecture](#database-architecture)
8. [Security Components](#security-components)
9. [External Dependencies](#external-dependencies)

---

## Backend Architecture Overview

The Campus Events & Ticketing System backend follows a **layered architecture** pattern implemented in ASP.NET Core. The backend is responsible for:

- **Business Logic Processing**: Event creation, ticket claiming, user management
- **Data Persistence**: Entity Framework Core with SQLite database
- **Authentication & Authorization**: Session-based authentication with BCrypt password hashing
- **Service Integration**: QR code generation, CSV export, email notifications
- **API Layer**: Page Model handlers serving Razor Pages frontend

**Architecture Style**: Monolithic Layered Architecture with clear separation of concerns

---

## High-Level Backend Architecture

<img width="1091" height="1881" alt="BACKEND drawio" src="https://github.com/user-attachments/assets/b5cb0a67-49b6-44e6-a37f-feff69d9f8ce" />

## Backend Component Breakdown

### 1. Application Entry Point (Program.cs)

**Purpose**: Configure and bootstrap the ASP.NET Core application

**Key Responsibilities**:
- **Dependency Injection Setup**: Register all services in the DI container
- **Middleware Pipeline**: Configure request processing pipeline
- **Database Context**: Set up Entity Framework Core with SQLite
- **Session Management**: Configure session state with cookie options
- **Authentication**: Set up authentication scheme and policies

**Code Location**: `/Program.cs`

**Configuration Details**:
```
Services Registered:
├── DbContext (Scoped)
│   └── AppDbContext with SQLite connection string
├── Session (Singleton)
│   ├── IdleTimeout: 20 minutes
│   ├── Cookie.HttpOnly: true
│   └── Cookie.IsEssential: true
├── Services (Transient)
│   └── DbCSVCommunicator
└── Razor Pages (Scoped)
    └── Runtime compilation enabled (dev only)

Middleware Pipeline Order:
1. Exception Handler (/Error)
2. HTTPS Redirection
3. Static Files (wwwroot)
4. Routing
5. Session Management
6. Authorization
7. Endpoint Mapping (Razor Pages)
```

---

### 2. Middleware Pipeline

**Purpose**: Process HTTP requests through a series of components

#### Middleware Components:

**A. Exception Handler Middleware**
- **Function**: Catch unhandled exceptions and display error page
- **Configuration**: `/Error` page for production
- **Location**: Built-in ASP.NET Core middleware

**B. HTTPS Redirection Middleware**
- **Function**: Redirect HTTP requests to HTTPS
- **Configuration**: Automatic redirection to port 7295
- **Location**: Built-in ASP.NET Core middleware

**C. Static Files Middleware**
- **Function**: Serve static files (CSS, JS, images, QR codes)
- **Configuration**: wwwroot directory
- **Location**: Built-in ASP.NET Core middleware

**D. Routing Middleware**
- **Function**: Match incoming requests to endpoints
- **Configuration**: Razor Pages convention-based routing
- **Location**: Built-in ASP.NET Core middleware

**E. Session Middleware**
- **Function**: Enable session state for authentication
- **Configuration**: In-memory session store (20-minute timeout)
- **Storage**: `HttpContext.Session` dictionary
- **Keys Used**:
  - `UserId` (int): Currently logged-in user ID
  - `UserRole` (int): User's role (0=Student, 1=Organizer, 2=Admin)

**F. Authorization Middleware**
- **Function**: Enforce authorization policies
- **Configuration**: Role-based access control in PageModels
- **Implementation**: Manual checks in OnGet/OnPost methods

**G. Endpoint Routing Middleware**
- **Function**: Execute matched Razor Page handlers
- **Configuration**: Maps Razor Pages and fallback to Index
- **Location**: Built-in ASP.NET Core middleware

---

### 3. Presentation/Controller Layer (Page Models)

**Purpose**: Handle HTTP requests and orchestrate business logic

#### Page Model Architecture:

```
PageModel Base Class (Microsoft.AspNetCore.Mvc.RazorPages)
│
├── Student Pages
│   ├── HomeModel
│   │   ├── OnGetAsync() → Load approved events with filters
│   │   └── Properties: Events, SearchQuery, CategoryFilter
│   │
│   ├── EventDetailsModel
│   │   ├── OnGetAsync(int id) → Load event details
│   │   ├── OnPostClaimTicketAsync(int id) → Claim ticket, generate QR
│   │   ├── OnPostSaveEventAsync(int id) → Save to calendar
│   │   └── OnPostUnsaveEventAsync(int id) → Remove from calendar
│   │
│   ├── TicketsModel
│   │   ├── OnGetAsync() → Load user's tickets
│   │   └── Properties: Tickets (with QR codes)
│   │
│   └── SavedEventsModel
│       ├── OnGetAsync() → Load user's saved events
│       └── Properties: SavedEvents
│
├── Organizer Pages
│   ├── HomeModel
│   │   ├── OnGetAsync() → Load organizer dashboard stats
│   │   └── Properties: TotalEvents, TicketsSold, UpcomingEvents
│   │
│   ├── CreateEventModel
│   │   ├── OnGetAsync() → Load organizations
│   │   ├── OnPostAsync() → Create new event (pending approval)
│   │   └── Validation: Required fields, date constraints
│   │
│   ├── EventDetailsModel
│   │   ├── OnGetAsync(int id) → Load event details
│   │   ├── OnPostExportCSVAsync(int id) → Generate CSV
│   │   ├── OnPostScanQRAsync(int id, IFormFile file) → Validate QR
│   │   └── Authorization: Check event ownership
│   │
│   └── EventsModel
│       ├── OnGetAsync() → Load organizer's events
│       └── Properties: Events (all, approved, pending, rejected)
│
└── Admin Pages
    ├── HomeModel
    │   ├── OnGetAsync() → Load global analytics
    │   └── Properties: TotalEvents, TotalUsers, Trends
    │
    ├── UsersModel
    │   ├── OnGetAsync() → Load users with filters
    │   ├── OnPostApproveAsync(int id) → Approve user
    │   ├── OnPostRejectAsync(int id) → Reject user
    │   ├── OnPostDeleteAsync(int id) → Delete user
    │   └── OnPostCreateAdminAsync() → Create admin account
    │
    ├── EventsModel
    │   ├── OnGetAsync() → Load events by status
    │   ├── OnPostApproveAsync(int id) → Approve event
    │   └── OnPostRejectAsync(int id) → Reject event
    │
    └── OrganizationsModel
        ├── OnGetAsync() → Load all organizations
        ├── OnPostCreateAsync() → Create organization
        ├── OnPostEditAsync(int id) → Update organization
        └── OnPostDeleteAsync(int id) → Delete organization
```

#### Page Model Responsibilities:

**Request Handling**:
- Process GET requests (load data for display)
- Process POST requests (handle form submissions)
- Validate user input (model binding + custom validation)
- Return appropriate results (Page, RedirectToPage, File)

**Session Management**:
- Check authentication state (`HttpContext.Session.GetInt32("UserId")`)
- Verify user role authorization
- Redirect unauthenticated users to login
- Redirect unauthorized users to appropriate dashboards

**Business Logic Orchestration**:
- Query database through DbContext
- Invoke domain services (CSV export, QR generation)
- Apply business rules (capacity checks, approval workflows)
- Manage transactions (multi-step operations)

**Error Handling**:
- Try-catch blocks around database operations
- Logging critical errors to console/file
- User-friendly error messages via TempData
- Validation error display via ModelState

---

### 4. Business Logic Layer

**Purpose**: Implement core business rules and domain logic

#### A. Domain Services

**DbCSVCommunicator Service**

**Location**: `/Data/DbCSVCommunicator.cs`

**Purpose**: Export event attendee data to CSV format

**Methods**:
```
public class DbCSVCommunicator
{
    private readonly AppDbContext _context;

    Constructor:
    └── DbCSVCommunicator(AppDbContext context)

    Public Methods:
    ├── ExportEventAttendeesToCSV(int eventId)
    │   ├── Query tickets for event with Include(User)
    │   ├── Build CSV string with headers
    │   ├── Add UTF-8 BOM for Excel compatibility
    │   └── Return byte[] for file download
    │
    ├── GenerateAttendanceReport(int eventId)
    │   ├── Calculate attendance statistics
    │   ├── Group by claim date for trends
    │   └── Return report data structure
    │
    └── Test() [Development Only]
        ├── Seed initial admin account
        ├── Seed sample organizations
        └── Seed test events and tickets
}
```

**CSV Format**:
```
Ticket ID, User Name, Email, Claimed Date, Claimed Time, Redeemed, Redeemed Date, Redeemed Time, Unique Code
123, "John Doe", "john@example.com", 2025-10-15, 14:30, Yes, 2025-10-20, 18:00, "abc-def-ghi"
```

**QR Code Service** (Integrated in Page Models)

**Location**: Distributed across Page Models (primarily `/Pages/Student/EventDetails.cshtml.cs`)

**Purpose**: Generate and validate QR codes for tickets

**Methods**:
```
QR Code Generation Flow:
├── 1. Generate unique GUID for ticket
│      string uniqueCode = Guid.NewGuid().ToString()
│
├── 2. Create QR Code Generator instance
│      QRCodeGenerator qrGenerator = new QRCodeGenerator()
│
├── 3. Create QR Code Data with error correction
│      QRCodeData qrCodeData = qrGenerator.CreateQrCode(
│          uniqueCode,
│          QRCodeGenerator.ECCLevel.Q
│      )
│
├── 4. Generate PNG byte array
│      PngByteQRCode qrCode = new PngByteQRCode(qrCodeData)
│      byte[] qrCodeImage = qrCode.GetGraphic(20)
│
└── 5. Convert to Base64 for database storage
       string qrCodeBase64 = Convert.ToBase64String(qrCodeImage)
```

**QR Code Validation Flow** (Organizer Scan):
```
QR Validation Process:
├── 1. Upload QR code image file
│      IFormFile qrFile
│
├── 2. Decode QR code to retrieve unique code
│      [Library: ZXing.Net or manual processing]
│
├── 3. Query ticket by unique code
│      var ticket = await _context.Tickets
│          .FirstOrDefaultAsync(t => t.UniqueCode == decodedCode)
│
├── 4. Validate ticket status
│      if (ticket.IsRedeemed) → Already used
│      if (ticket.EventId != currentEventId) → Wrong event
│
├── 5. Mark as redeemed
│      ticket.IsRedeemed = true
│      ticket.RedeemedAt = DateTime.UtcNow
│      await _context.SaveChangesAsync()
│
└── 6. Return success message
       "Ticket validated successfully for [User Name]"
```

**Authentication Service** (Integrated in Login/Signup)

**Location**: `/Pages/Login.cshtml.cs`, `/Pages/SignupStudent.cshtml.cs`, `/Pages/SignupOrganizer.cshtml.cs`

**Purpose**: Handle user authentication and password security

**Methods**:
```
Authentication Flow:

Registration (Signup):
├── 1. Collect user input (email, password, name)
├── 2. Validate input
│      - Email format validation
│      - Password strength requirements (min 8 chars)
│      - Check email uniqueness
├── 3. Hash password with BCrypt
│      string passwordHash = BCrypt.Net.BCrypt.HashPassword(password)
├── 4. Create User entity
│      new User {
│          Email = email,
│          PasswordHash = passwordHash,
│          Name = name,
│          Role = UserRole.Student, // or Organizer
│          ApprovalStatus = ApprovalStatus.Approved, // or Pending
│          CreatedAt = DateTime.UtcNow
│      }
├── 5. Save to database
│      _context.Users.Add(user)
│      await _context.SaveChangesAsync()
└── 6. Create session (if auto-approved)
       HttpContext.Session.SetInt32("UserId", user.Id)

Login:
├── 1. Collect credentials (email, password)
├── 2. Query user by email
│      var user = await _context.Users
│          .FirstOrDefaultAsync(u => u.Email == email)
├── 3. Verify password with BCrypt
│      bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)
├── 4. Check approval status
│      if (user.ApprovalStatus != Approved) → Reject
├── 5. Create session
│      HttpContext.Session.SetInt32("UserId", user.Id)
│      HttpContext.Session.SetInt32("UserRole", (int)user.Role)
└── 6. Redirect to role-specific dashboard
```

#### B. Business Rules Engine

**Location**: Distributed across Page Models (validation logic)

**Purpose**: Enforce business constraints and workflows

**Key Business Rules**:

**1. Ticket Claiming Rules**:
```
Rule: Student can claim ticket for an event
Conditions:
├── User is authenticated
├── User role is Student
├── Event is approved (ApprovalStatus == Approved)
├── Event date is in the future
├── Event has available capacity (TicketsIssued < Capacity)
├── User hasn't already claimed ticket for this event
└── If paid event, payment processed (mock for now)

Implementation Location: /Pages/Student/EventDetails.cshtml.cs → OnPostClaimTicketAsync
```

**2. Event Creation Rules**:
```
Rule: Organizer can create an event
Conditions:
├── User is authenticated
├── User role is Organizer
├── User approval status is Approved
├── All required fields provided (title, date, location, capacity)
├── Event date is in the future
├── Capacity is positive integer
└── Price is non-negative decimal

Post-Creation:
└── Event created with ApprovalStatus = Pending (requires admin approval)

Implementation Location: /Pages/Organizer/CreateEvent.cshtml.cs → OnPostAsync
```

**3. User Approval Rules**:
```
Rule: Admin can approve organizer accounts
Conditions:
├── User is authenticated
├── User role is Admin
├── Target user exists
├── Target user role is Organizer
└── Target user status is Pending

Actions:
├── Update ApprovalStatus to Approved
└── User can now create events

Implementation Location: /Pages/Admin/Users.cshtml.cs → OnPostApproveAsync
```

**4. Event Moderation Rules**:
```
Rule: Admin can moderate events
Conditions:
├── User is authenticated
├── User role is Admin
├── Event exists
└── Event status is Pending

Actions:
├── Approve: Event visible to students
└── Reject: Event hidden, organizer notified

Implementation Location: /Pages/Admin/Events.cshtml.cs → OnPostApproveAsync/OnPostRejectAsync
```

**5. CSV Export Rules**:
```
Rule: Organizer can export attendee list
Conditions:
├── User is authenticated
├── User role is Organizer
├── Event exists
├── User owns the event (Event.OrganizerId == User.Id)
└── Event has tickets issued

Implementation Location: /Pages/Organizer/EventDetails.cshtml.cs → OnPostExportCSVAsync
```

**6. QR Validation Rules**:
```
Rule: Organizer can validate ticket QR codes
Conditions:
├── User is authenticated
├── User role is Organizer
├── Event exists
├── User owns the event
├── Ticket exists
├── Ticket belongs to the event
└── Ticket not already redeemed

Actions:
├── Mark ticket as redeemed
└── Record redemption timestamp

Implementation Location: /Pages/Organizer/EventDetails.cshtml.cs → OnPostScanQRAsync
```

---

### 5. Data Access Layer

**Purpose**: Abstract database operations using Entity Framework Core

#### A. AppDbContext

**Location**: `/Data/AppDbContext.cs`

**Purpose**: Configure database schema and manage database operations

**Class Structure**:
```
public class AppDbContext : DbContext
{
    // DbSets (Entity Collections)
    ├── public DbSet<User> Users { get; set; }
    ├── public DbSet<Event> Events { get; set; }
    ├── public DbSet<Ticket> Tickets { get; set; }
    ├── public DbSet<Organization> Organizations { get; set; }
    └── public DbSet<SavedEvent> SavedEvents { get; set; }

    // Constructor
    ├── public AppDbContext(DbContextOptions<AppDbContext> options)
    │       : base(options)

    // Configuration
    └── protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Entity Configurations
            ├── Configure relationships (foreign keys)
            ├── Configure indexes for performance
            ├── Configure constraints (unique, required)
            └── Configure cascade delete behaviors
        }
}
```

**Entity Configurations**:

**User Entity Configuration**:
```
modelBuilder.Entity<User>(entity =>
{
    // Primary Key
    entity.HasKey(u => u.Id);

    // Indexes for performance
    entity.HasIndex(u => u.Email).IsUnique();
    entity.HasIndex(u => u.Role);
    entity.HasIndex(u => u.ApprovalStatus);

    // Relationships
    entity.HasOne<Organization>()
          .WithMany()
          .HasForeignKey(u => u.OrganizationId)
          .OnDelete(DeleteBehavior.SetNull);

    // Constraints
    entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
    entity.Property(u => u.PasswordHash).IsRequired();
    entity.Property(u => u.Name).IsRequired().HasMaxLength(200);
});
```

**Event Entity Configuration**:
```
modelBuilder.Entity<Event>(entity =>
{
    // Primary Key
    entity.HasKey(e => e.Id);

    // Indexes
    entity.HasIndex(e => e.EventDate);
    entity.HasIndex(e => e.ApprovalStatus);
    entity.HasIndex(e => e.OrganizerId);

    // Relationships
    entity.HasOne<User>()
          .WithMany()
          .HasForeignKey(e => e.OrganizerId)
          .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne<Organization>()
          .WithMany()
          .HasForeignKey(e => e.OrganizationId)
          .OnDelete(DeleteBehavior.SetNull);

    // Constraints
    entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
    entity.Property(e => e.Location).IsRequired().HasMaxLength(300);
    entity.Property(e => e.Capacity).IsRequired();
    entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
});
```

**Ticket Entity Configuration**:
```
modelBuilder.Entity<Ticket>(entity =>
{
    // Primary Key
    entity.HasKey(t => t.Id);

    // Indexes
    entity.HasIndex(t => t.UniqueCode).IsUnique();
    entity.HasIndex(t => t.UserId);
    entity.HasIndex(t => t.EventId);
    entity.HasIndex(t => t.IsRedeemed);

    // Relationships
    entity.HasOne<User>()
          .WithMany()
          .HasForeignKey(t => t.UserId)
          .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne<Event>()
          .WithMany()
          .HasForeignKey(t => t.EventId)
          .OnDelete(DeleteBehavior.Cascade);

    // Constraints
    entity.Property(t => t.UniqueCode).IsRequired().HasMaxLength(100);
    entity.Property(t => t.QrCodeBase64).IsRequired();
});
```

#### B. Entity Framework Core Features Used

**LINQ Query Translation**:
```
C# LINQ Query:
var events = await _context.Events
    .Include(e => e.Organization)
    .Where(e => e.ApprovalStatus == ApprovalStatus.Approved)
    .Where(e => e.EventDate > DateTime.UtcNow)
    .OrderBy(e => e.EventDate)
    .ToListAsync();

Translated SQL:
SELECT e.*, o.*
FROM Events e
LEFT JOIN Organizations o ON e.OrganizationId = o.Id
WHERE e.ApprovalStatus = 1
  AND e.EventDate > '2025-11-03'
ORDER BY e.EventDate
```

**Change Tracking**:
```
EF Core tracks entity state:
├── Added: New entity to be inserted
├── Modified: Existing entity with changes
├── Deleted: Entity to be removed
├── Unchanged: No changes detected
└── Detached: Not tracked

Example:
var event = await _context.Events.FindAsync(id);
event.TicketsIssued++;  // EF Core detects modification
await _context.SaveChangesAsync();  // Generates UPDATE SQL
```

**Transaction Management**:
```
Implicit Transactions (default):
await _context.SaveChangesAsync();
// All changes saved in one atomic transaction

Explicit Transactions:
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    _context.Tickets.Add(newTicket);
    await _context.SaveChangesAsync();

    event.TicketsIssued++;
    await _context.SaveChangesAsync();

    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

**Migration Management**:
```
Migrations track schema changes over time:

Migration Files (in /Migrations folder):
├── 20250901120000_InitialCreate.cs
│   └── Creates Users, Events, Organizations tables
├── 20250915143000_AddTicketsTable.cs
│   └── Creates Tickets table with QR code field
├── 20251001095000_AddSavedEventsTable.cs
│   └── Creates SavedEvents join table
└── 20251015161000_AddOrganizationFK.cs
    └── Adds OrganizationId to Events table

Commands:
├── dotnet ef migrations add <Name>  → Create new migration
├── dotnet ef database update        → Apply migrations to DB
└── dotnet ef migrations remove      → Undo last migration
```

---

### 6. Database Architecture

**Purpose**: Persistent data storage using SQLite

#### Database Schema

```
SQLite Database: campusevents.db
Location: Project root directory

Tables:
├── Users (7 columns + relationships)
├── Events (14 columns + relationships)
├── Tickets (7 columns + relationships)
├── Organizations (6 columns + relationships)
├── SavedEvents (4 columns + join table)
└── __EFMigrationsHistory (tracking table)
```

**Complete Schema Diagram**:

<img width="946" height="1254" alt="schemaDiagram" src="https://github.com/user-attachments/assets/419d333d-15f4-4230-945c-438ffcd74e2b" />

#### Database Connection Configuration

**Connection String** (in `appsettings.json`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=campusevents.db"
  }
}
```

**Configuration in Program.cs**:
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);
```

**SQLite Advantages for Prototype**:
- File-based (no server required)
- Zero configuration
- Cross-platform compatible
- Lightweight (~600KB database file)
- Fast for read-heavy workloads
- Easy backup (copy file)

**SQLite Limitations**:
- Single-writer constraint
- No built-in replication
- Limited concurrent write performance
- Max database size: 281 TB (sufficient for prototype)

---

### 7. Security Components

**Purpose**: Protect user data and prevent unauthorized access

#### A. Authentication System

**Password Security**:
```
Password Hashing with BCrypt:

Hash Generation (Signup):
├── Input: Plain text password from user
├── Algorithm: BCrypt with work factor 10
├── Process:
│   ├── Generate random salt
│   ├── Hash password with salt
│   └── Store hash (includes salt)
└── Output: Hashed password string
    Example: "$2a$10$N9qo8uLOickgx2ZMRZoMye..."

Hash Verification (Login):
├── Input: Plain text password + stored hash
├── Process:
│   ├── Extract salt from stored hash
│   ├── Hash input password with same salt
│   └── Compare result with stored hash
└── Output: Boolean (match or no match)

Code Location: /Pages/Login.cshtml.cs, /Pages/Signup*.cshtml.cs

Advantages:
├── Slow by design (prevents brute force)
├── Includes salt (prevents rainbow table attacks)
├── Adaptive (can increase work factor over time)
└── One-way (cannot decrypt)
```

**Session Management**:
```
Session Architecture:

Session Storage (In-Memory):
├── Key-Value Store: Dictionary<string, byte[]>
├── Session ID: Randomly generated cookie value
├── Storage Location: Server memory (IIS process)
├── Timeout: 20 minutes of inactivity
└── Data Stored:
    ├── "UserId" → int (User primary key)
    └── "UserRole" → int (0=Student, 1=Organizer, 2=Admin)

Session Cookie Configuration:
├── Name: .AspNetCore.Session
├── HttpOnly: true (prevents JavaScript access)
├── IsEssential: true (always sent)
├── SameSite: Lax (CSRF protection)
├── Secure: true (HTTPS only in production)
└── Expires: Session (deleted when browser closes)

Session Creation (Login):
HttpContext.Session.SetInt32("UserId", user.Id);
HttpContext.Session.SetInt32("UserRole", (int)user.Role);

Session Validation (Every Request):
var userId = HttpContext.Session.GetInt32("UserId");
if (userId == null)
{
    return RedirectToPage("/Login");
}

Session Destruction (Logout):
HttpContext.Session.Clear();

Code Locations:
├── Configuration: /Program.cs (lines 8-14)
├── Creation: /Pages/Login.cshtml.cs
├── Validation: All protected page models (OnGet methods)
└── Destruction: /Pages/Logout.cshtml.cs
```

#### B. Authorization System

**Role-Based Access Control (RBAC)**:
```
Authorization Flow:

1. Page Request
   ↓
2. Check Session Exists
   if (HttpContext.Session.GetInt32("UserId") == null)
       → Redirect to /Login
   ↓
3. Load User from Database
   var user = await _context.Users.FindAsync(userId.Value)
   ↓
4. Check User Role
   if (user.Role != UserRole.Admin)  // Example for admin page
       → Redirect to appropriate dashboard
   ↓
5. Check Approval Status (for Organizers)
   if (user.ApprovalStatus != ApprovalStatus.Approved)
       → Show "Pending Approval" message
   ↓
6. Check Resource Ownership (for actions)
   if (event.OrganizerId != userId.Value)  // Example for editing event
       → Return Unauthorized or RedirectToPage
   ↓
7. Allow Access
   return Page()

Implementation Pattern (in Page Models):
public async Task<IActionResult> OnGetAsync()
{
    // Step 2: Check authentication
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
    {
        return RedirectToPage("/Login");
    }

    // Step 3: Load user
    var user = await _context.Users.FindAsync(userId.Value);
    if (user == null)
    {
        HttpContext.Session.Clear();
        return RedirectToPage("/Login");
    }

    // Step 4: Check role
    if (user.Role != UserRole.Admin)  // For admin pages
    {
        return RedirectToPage("/Index");
    }

    // Step 5: Check approval (if applicable)
    if (user.Role == UserRole.Organizer &&
        user.ApprovalStatus != ApprovalStatus.Approved)
    {
        // Show pending approval message
    }

    // Step 7: Proceed
    // Load data and return Page()
}
```

**Resource-Level Authorization**:
```
Ownership Verification (Example: Edit Event):

1. User requests to edit Event ID 42
   ↓
2. Load event from database
   var event = await _context.Events.FindAsync(42)
   ↓
3. Verify ownership
   if (event.OrganizerId != currentUserId)
       → Access Denied
   ↓
4. Allow edit

Code Example:
public async Task<IActionResult> OnPostAsync(int id)
{
    var userId = HttpContext.Session.GetInt32("UserId");
    var eventData = await _context.Events.FindAsync(id);

    if (eventData.OrganizerId != userId.Value)
    {
        TempData["Error"] = "You don't have permission to edit this event";
        return RedirectToPage("/Organizer/Events");
    }

    // Proceed with update...
}
```

#### C. Input Validation & Security

**SQL Injection Prevention**:
```
EF Core Parameterized Queries (Automatic):

User Input:
email = "test@example.com'; DROP TABLE Users; --"

LINQ Query:
var user = await _context.Users
    .FirstOrDefaultAsync(u => u.Email == email);

Generated SQL (Safe):
SELECT * FROM Users
WHERE Email = @p0

Parameters:
@p0 = "test@example.com'; DROP TABLE Users; --"

Result: Query safely searches for literal string, no injection possible
```

**XSS Prevention**:
```
Razor Page Automatic Encoding:

User Input (malicious):
name = "<script>alert('XSS')</script>"

Razor Output:
<h1>@Model.Name</h1>

Rendered HTML (Safe):
<h1>&lt;script&gt;alert('XSS')&lt;/script&gt;</h1>

Browser Display:
<script>alert('XSS')</script>  (as plain text, not executed)
```

**CSRF Protection**:
```
Anti-Forgery Tokens (Automatic in Razor Pages):

Form Rendering:
<form method="post">
    <!-- ASP.NET Core automatically adds: -->
    <input name="__RequestVerificationToken"
           type="hidden"
           value="CfDJ8..." />
    <button type="submit">Submit</button>
</form>

Form Submission Validation:
POST /Page
├── Token from form: CfDJ8...
├── Token from cookie: CfDJ8...
├── Server validates both match
└── If mismatch → HTTP 400 Bad Request

Configuration: Enabled by default in Razor Pages
```

---

### 8. External Dependencies

**Purpose**: Leverage third-party libraries for specialized functionality

#### A. QRCoder Library

**Package**: `QRCoder` (NuGet)
**Version**: 1.4.x or later
**Purpose**: Generate QR codes for ticket validation

**Integration Architecture**:
```
QRCoder Library Architecture:

NuGet Package
├── QRCoder.dll
└── Dependencies: System.Drawing.Common

Classes Used:
├── QRCodeGenerator
│   ├── CreateQrCode(string data, ECCLevel level)
│   └── Generates QRCodeData object
│
└── PngByteQRCode
    ├── Constructor(QRCodeData data)
    └── GetGraphic(int pixelsPerModule) → byte[]

Error Correction Levels:
├── L (Low): 7% recovery
├── M (Medium): 15% recovery
├── Q (Quartile): 25% recovery  ← We use this
└── H (High): 30% recovery

Usage in Application:
Location: /Pages/Student/EventDetails.cshtml.cs (ticket claiming)

Code Flow:
1. Student claims ticket
2. Generate unique GUID
3. Create QR code from GUID
4. Convert to Base64 PNG
5. Store in database
6. Display on Tickets page

QR Code Content:
└── Ticket Unique Code (GUID)
    Example: "a1b2c3d4-e5f6-7890-abcd-ef1234567890"

QR Code Display:
└── Base64 PNG embedded in <img> tag
    <img src="data:image/png;base64,{QrCodeBase64}" />
```

#### B. BCrypt.Net Library

**Package**: `BCrypt.Net-Next` (NuGet)
**Version**: 4.0.x or later
**Purpose**: Secure password hashing

**Integration Architecture**:
```
BCrypt.Net Library Architecture:

NuGet Package
└── BCrypt.Net-Next.dll

Classes Used:
└── BCrypt (static class)
    ├── HashPassword(string input, int workFactor = 10) → string
    └── Verify(string input, string hash) → bool

Work Factor:
├── Default: 10
├── Meaning: 2^10 = 1024 iterations
├── Higher = slower = more secure
└── Recommended: 10-12 for web applications

Hash Format:
$2a$10$N9qo8uLOickgx2ZMRZoMyeFOCF...
│  │  │  └─ Hash and salt (combined)
│  │  └─ Salt (22 characters)
│  └─ Work factor (10)
└─ Algorithm version (2a)

Usage in Application:
Locations:
├── /Pages/SignupStudent.cshtml.cs
├── /Pages/SignupOrganizer.cshtml.cs
└── /Pages/Login.cshtml.cs

Signup (Hash):
string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
var user = new User { PasswordHash = passwordHash, ... };

Login (Verify):
var user = await _context.Users
    .FirstOrDefaultAsync(u => u.Email == email);
bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
```

#### C. Entity Framework Core SQLite Provider

**Package**: `Microsoft.EntityFrameworkCore.Sqlite` (NuGet)
**Version**: 9.0.x
**Purpose**: SQLite database provider for EF Core

**Integration Architecture**:
```
EF Core SQLite Provider:

NuGet Packages:
├── Microsoft.EntityFrameworkCore.Sqlite (9.0.x)
├── Microsoft.EntityFrameworkCore (9.0.x)
└── SQLitePCLRaw.bundle_e_sqlite3 (dependency)

Provider Registration:
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString)
);

Database File:
├── Location: Project root
├── Name: campusevents.db
├── Size: ~600KB (with seed data)
└── Connection String: "Data Source=campusevents.db"

Features Used:
├── Migrations (schema versioning)
├── LINQ query translation
├── Change tracking
├── Transaction support
└── Connection pooling

Migrations:
├── Stored in /Migrations folder
├── Applied via dotnet ef database update
└── History tracked in __EFMigrationsHistory table
```

#### D. Additional System Libraries

**System.IO** (File Operations):
```
Purpose: CSV file generation and download

Usage:
├── Encoding.UTF8.GetPreamble() → BOM for Excel
├── Encoding.UTF8.GetBytes() → Convert string to bytes
└── File() result for downloading

Location: /Pages/Organizer/EventDetails.cshtml.cs
```

**System.Security.Cryptography** (Session IDs):
```
Purpose: Generate cryptographically secure random session IDs

Usage: Automatic by ASP.NET Core session middleware

Generation: RandomNumberGenerator.GetBytes()
```

---

## Data Flow Diagrams

### Use Case 1: Student Claims Ticket

```
[Student Browser]
       │
       │ 1. HTTP GET /Student/EventDetails?id=5
       ▼
[ASP.NET Core Middleware Pipeline]
       │
       │ 2. Route to EventDetailsModel.OnGetAsync(5)
       ▼
[EventDetailsModel (Page Model)]
       │
       │ 3. Check session authentication
       │    var userId = HttpContext.Session.GetInt32("UserId")
       │
       │ 4. Query event from database
       ▼
[AppDbContext (EF Core)]
       │
       │ 5. LINQ: _context.Events.Include(o).Where(id==5).FirstOrDefaultAsync()
       ▼
[SQLite Database]
       │
       │ 6. Return Event entity
       ▼
[EventDetailsModel]
       │
       │ 7. Check if user already has ticket
       │    _context.Tickets.Any(t => t.UserId == userId && t.EventId == 5)
       │
       │ 8. Render page with "Claim Ticket" button
       ▼
[Student Browser]
       │
       │ 9. User clicks "Claim Ticket" button
       │    HTTP POST /Student/EventDetails (id=5)
       ▼
[EventDetailsModel.OnPostClaimTicketAsync(5)]
       │
       │ 10. Validate capacity
       │     if (event.TicketsIssued >= event.Capacity) → Error
       │
       │ 11. Generate unique ticket code
       │     string uniqueCode = Guid.NewGuid().ToString()
       ▼
[QRCoder Library]
       │
       │ 12. Generate QR code PNG
       │     QRCodeGenerator.CreateQrCode(uniqueCode)
       │     PngByteQRCode.GetGraphic(20)
       │
       │ 13. Convert to Base64
       │     Convert.ToBase64String(qrCodeImage)
       ▼
[EventDetailsModel]
       │
       │ 14. Create Ticket entity
       │     new Ticket {
       │         UserId = userId,
       │         EventId = 5,
       │         UniqueCode = uniqueCode,
       │         QrCodeBase64 = base64String,
       │         ClaimedAt = DateTime.UtcNow,
       │         IsRedeemed = false
       │     }
       │
       │ 15. Update event tickets issued
       │     event.TicketsIssued++
       │
       │ 16. Save to database (in transaction)
       ▼
[AppDbContext]
       │
       │ 17. EF Core SaveChangesAsync()
       │     BEGIN TRANSACTION
       │     INSERT INTO Tickets (...)
       │     UPDATE Events SET TicketsIssued = TicketsIssued + 1
       │     COMMIT TRANSACTION
       ▼
[SQLite Database]
       │
       │ 18. Return success
       ▼
[EventDetailsModel]
       │
       │ 19. Set success message
       │     TempData["Success"] = "Ticket claimed successfully!"
       │
       │ 20. Redirect to same page
       │     RedirectToPage(new { id = 5 })
       ▼
[Student Browser]
       │
       │ 21. Page reloads, now shows ticket claimed
       └─────> Display success message & QR code
```

---

### Use Case 2: Organizer Exports CSV

```
[Organizer Browser]
       │
       │ 1. HTTP GET /Organizer/EventDetails?id=10
       ▼
[EventDetailsModel (Organizer)]
       │
       │ 2. Check authentication & ownership
       │    var userId = HttpContext.Session.GetInt32("UserId")
       │    if (event.OrganizerId != userId) → Unauthorized
       │
       │ 3. Load event with ticket count
       ▼
[AppDbContext]
       │
       │ 4. Query event and tickets
       │    _context.Events.Include(Tickets).ThenInclude(User)
       ▼
[SQLite Database]
       │
       │ 5. Return Event with Tickets collection
       ▼
[EventDetailsModel]
       │
       │ 6. Render page with "Export CSV" button
       ▼
[Organizer Browser]
       │
       │ 7. User clicks "Export CSV" button
       │    HTTP POST /Organizer/EventDetails (handler=ExportCSV, id=10)
       ▼
[EventDetailsModel.OnPostExportCSVAsync(10)]
       │
       │ 8. Query tickets for event
       ▼
[AppDbContext]
       │
       │ 9. LINQ: _context.Tickets
       │          .Include(t => t.User)
       │          .Where(t => t.EventId == 10)
       │          .OrderBy(t => t.ClaimedAt)
       │          .ToListAsync()
       ▼
[SQLite Database]
       │
       │ 10. Return List<Ticket> with User data
       ▼
[DbCSVCommunicator Service]
       │
       │ 11. Build CSV string
       │     StringBuilder csv = new()
       │     csv.AppendLine("Ticket ID,User Name,Email,...")
       │
       │ 12. Loop through tickets
       │     foreach (ticket in tickets)
       │         csv.AppendLine($"{ticket.Id},\"{ticket.User.Name}\",...")
       │
       │ 13. Add UTF-8 BOM for Excel compatibility
       │     byte[] bom = Encoding.UTF8.GetPreamble()
       │     byte[] csvBytes = Encoding.UTF8.GetBytes(csv.ToString())
       │     byte[] result = bom + csvBytes
       │
       │ 14. Generate filename with timestamp
       │     string fileName = $"{event.Title}_Attendees_{DateTime:yyyyMMdd_HHmmss}.csv"
       ▼
[EventDetailsModel]
       │
       │ 15. Return file result
       │     return File(bytesWithBOM, "text/csv", fileName)
       ▼
[ASP.NET Core]
       │
       │ 16. HTTP Response with file
       │     Content-Type: text/csv
       │     Content-Disposition: attachment; filename="Event_Attendees_20251103_143000.csv"
       ▼
[Organizer Browser]
       │
       │ 17. Browser downloads file
       │     Save to Downloads folder or show download bar
       │
       │ 18. User opens file in Excel
       └─────> CSV displays correctly with proper date/time formatting
```

---

### Use Case 3: Admin Approves Organizer Account

```
[Admin Browser]
       │
       │ 1. HTTP GET /Admin/Users
       ▼
[UsersModel (Page Model)]
       │
       │ 2. Check admin authentication
       │    var userId = HttpContext.Session.GetInt32("UserId")
       │    var user = _context.Users.Find(userId)
       │    if (user.Role != UserRole.Admin) → Unauthorized
       │
       │ 3. Query users with filters
       │    var query = _context.Users.AsQueryable()
       │    if (RoleFilter == "Organizer")
       │        query = query.Where(u => u.Role == UserRole.Organizer)
       │    if (StatusFilter == "Pending")
       │        query = query.Where(u => u.ApprovalStatus == ApprovalStatus.Pending)
       ▼
[AppDbContext]
       │
       │ 4. Execute LINQ query
       ▼
[SQLite Database]
       │
       │ 5. Return filtered user list
       │    [User1: Organizer, Pending]
       │    [User2: Organizer, Pending]
       │    [User3: Student, Approved]
       ▼
[UsersModel]
       │
       │ 6. Render page with user list
       │    Show Approve/Reject buttons for pending users
       ▼
[Admin Browser]
       │
       │ 7. Admin clicks "Approve" button for User1
       │    HTTP POST /Admin/Users (handler=Approve, id=123)
       ▼
[UsersModel.OnPostApproveAsync(123)]
       │
       │ 8. Load user from database
       ▼
[AppDbContext]
       │
       │ 9. Query user by ID
       │    var user = await _context.Users.FindAsync(123)
       ▼
[SQLite Database]
       │
       │ 10. Return User entity (ID=123, Status=Pending)
       ▼
[UsersModel]
       │
       │ 11. Update approval status
       │     user.ApprovalStatus = ApprovalStatus.Approved
       │
       │ 12. Save changes
       ▼
[AppDbContext]
       │
       │ 13. EF Core change tracking detects modification
       │     SaveChangesAsync() generates:
       │     UPDATE Users SET ApprovalStatus = 1 WHERE Id = 123
       ▼
[SQLite Database]
       │
       │ 14. Execute UPDATE statement
       │     User ID 123 now has ApprovalStatus = Approved
       ▼
[UsersModel]
       │
       │ 15. Set success message
       │     TempData["Success"] = "User approved successfully"
       │
       │ 16. Redirect to Users page
       │     RedirectToPage()
       ▼
[Admin Browser]
       │
       │ 17. Page reloads with updated user list
       │     User1 now shows "Approved" badge (green)
       │     User can now create events
       └─────> Display success message
```

---

## Summary

This backend architecture provides a **solid foundation** for the Campus Events & Ticketing System with:

✅ **Clear Separation of Concerns**: Layered architecture with distinct responsibilities
✅ **Security First**: BCrypt password hashing, session management, input validation
✅ **Scalable Design**: EF Core abstraction allows easy database migration
✅ **Service-Oriented**: Reusable services (CSV export, QR generation)
✅ **Transaction Safety**: EF Core ensures data consistency
✅ **Performance Optimized**: Indexes on frequently queried columns
✅ **Maintainable Code**: Dependency injection, consistent patterns

### Key Strengths:
- Monolithic architecture appropriate for prototype scope
- Role-based access control implemented at multiple levels
- Business rules centralized in Page Models
- Database schema supports all required features
- External libraries properly integrated

### Future Enhancements:
- Migrate from SQLite to PostgreSQL for production
- Implement repository pattern for better testability
- Add distributed caching (Redis) for session management
- Extract business logic into dedicated service layer
- Implement API layer for mobile app support

---

**Document Version:** 1.0
**Created:** November 3, 2025
**Team:** MONDAY_FK (Backend Team)
**Members:** Kevin Ung, Salvatore Bruzzese, Dmitrii Cazacu, Souleymane Camara, Nand Patel
**Course:** SOEN 341 - Software Process (Fall 2025)
**Sprint:** Sprint 3

