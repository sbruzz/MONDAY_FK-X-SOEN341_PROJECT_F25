# System Architecture Documentation
**Campus Events & Ticketing System**
**SOEN 341 Fall 2025 - Team MONDAY_FK**

---

## Table of Contents
1. [Architecture Overview](#architecture-overview)
2. [System Architecture](#system-architecture)
3. [Technology Stack](#technology-stack)
4. [Data Model](#data-model)
5. [Application Layers](#application-layers)
6. [Security Architecture](#security-architecture)
7. [Deployment Architecture](#deployment-architecture)
8. [Design Patterns](#design-patterns)
9. [API Design](#api-design)
10. [Scalability Considerations](#scalability-considerations)

---

## Architecture Overview

### Architectural Style
The Campus Events & Ticketing System follows a **Layered Monolithic Architecture** using the **Model-View-PageModel (MVPM)** pattern, implemented through ASP.NET Core Razor Pages. This architecture was chosen for the following reasons:

- **Simplicity**: Appropriate for a middle-fidelity prototype with clear bounded context
- **Development Speed**: Faster initial development and deployment
- **Team Familiarity**: Lower learning curve for team members
- **Maintainability**: Clear separation of concerns with well-defined layers
- **Testability**: Each layer can be tested independently

### High-Level Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                     Presentation Layer                       │
│  ┌────────────┐  ┌────────────┐  ┌────────────────────────┐│
│  │  Student   │  │ Organizer  │  │  Administrator         ││
│  │   Pages    │  │   Pages    │  │     Pages              ││
│  └────────────┘  └────────────┘  └────────────────────────┘│
│       Razor Pages (.cshtml) + Page Models (.cshtml.cs)      │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     Business Logic Layer                     │
│  ┌────────────────┐  ┌──────────────┐  ┌─────────────────┐ │
│  │ Page Models    │  │  Services    │  │  Validation     │ │
│  │ (Controllers)  │  │  - CSV Export│  │  - Input Rules  │ │
│  └────────────────┘  └──────────────┘  └─────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                       Data Access Layer                      │
│  ┌────────────────┐  ┌──────────────────────────────────┐  │
│  │  AppDbContext  │  │   Entity Framework Core          │  │
│  │  (DbContext)   │  │   - LINQ Queries                 │  │
│  └────────────────┘  │   - Change Tracking              │  │
│                      │   - Migrations                    │  │
│                      └──────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                       Database Layer                         │
│                 SQLite Database (campusevents.db)            │
│    ┌────────┐ ┌────────┐ ┌────────┐ ┌──────────────────┐  │
│    │ Users  │ │ Events │ │Tickets │ │ Organizations    │  │
│    └────────┘ └────────┘ └────────┘ └──────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

---

## System Architecture

### Component Diagram

```
┌───────────────────────────────────────────────────────────────┐
│                      Web Application                          │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │                    ASP.NET Core Pipeline                 │ │
│  │  ┌──────────┐  ┌──────────┐  ┌─────────────────────┐  │ │
│  │  │Middleware│  │ Routing  │  │ Session Management  │  │ │
│  │  └──────────┘  └──────────┘  └─────────────────────┘  │ │
│  └─────────────────────────────────────────────────────────┘ │
│                                                               │
│  ┌───────────────────────────────────────────────────────┐  │
│  │               Page Components                          │  │
│  │  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐  │  │
│  │  │   Student    │ │  Organizer   │ │    Admin     │  │  │
│  │  │  - Home      │ │  - Home      │ │  - Home      │  │  │
│  │  │  - Events    │ │  - Events    │ │  - Users     │  │  │
│  │  │  - Tickets   │ │  - Analytics │ │  - Events    │  │  │
│  │  │  - Calendar  │ │  - Export    │ │  - Analytics │  │  │
│  │  └──────────────┘ └──────────────┘ └──────────────┘  │  │
│  └───────────────────────────────────────────────────────┘  │
│                                                               │
│  ┌───────────────────────────────────────────────────────┐  │
│  │                 Domain Models                          │  │
│  │  ┌──────┐ ┌──────┐ ┌────────┐ ┌──────────────────┐  │  │
│  │  │ User │ │Event │ │ Ticket │ │ Organization     │  │  │
│  │  └──────┘ └──────┘ └────────┘ └──────────────────┘  │  │
│  │  ┌────────────┐ ┌────────────────────────────────┐  │  │
│  │  │SavedEvent  │ │       Enums                    │  │  │
│  │  └────────────┘ │  - UserRole                    │  │  │
│  │                 │  - ApprovalStatus              │  │  │
│  │                 │  - EventCategory               │  │  │
│  │                 └────────────────────────────────┘  │  │
│  └───────────────────────────────────────────────────────┘  │
│                                                               │
│  ┌───────────────────────────────────────────────────────┐  │
│  │                    Services                            │  │
│  │  ┌──────────────────┐  ┌───────────────────────────┐ │  │
│  │  │ DbCSVCommunicator│  │   QRCoder Library         │ │  │
│  │  │ - ExportCSV      │  │   - QR Generation         │ │  │
│  │  │ - DataSeeding    │  │   - PNG Output            │ │  │
│  │  └──────────────────┘  └───────────────────────────┘ │  │
│  └───────────────────────────────────────────────────────┘  │
│                                                               │
│  ┌───────────────────────────────────────────────────────┐  │
│  │              Data Access (EF Core)                     │  │
│  │  ┌────────────────────────────────────────────────┐  │  │
│  │  │            AppDbContext                         │  │  │
│  │  │  - DbSet<User>                                  │  │  │
│  │  │  - DbSet<Event>                                 │  │  │
│  │  │  - DbSet<Ticket>                                │  │  │
│  │  │  - DbSet<Organization>                          │  │  │
│  │  │  - DbSet<SavedEvent>                            │  │  │
│  │  │  - SaveChangesAsync()                           │  │  │
│  │  └────────────────────────────────────────────────┘  │  │
│  └───────────────────────────────────────────────────────┘  │
└───────────────────────────────────────────────────────────────┘
                              │
                              ▼
                    ┌──────────────────┐
                    │  SQLite Database │
                    │ (campusevents.db)│
                    └──────────────────┘
```

---

## Technology Stack

### Core Technologies

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| **Framework** | ASP.NET Core | 9.0 | Web application framework |
| **UI** | Razor Pages | 9.0 | Server-side rendering |
| **ORM** | Entity Framework Core | 9.0 | Object-relational mapping |
| **Database** | SQLite | 3.x | Embedded relational database |
| **Language** | C# | 12.0 | Primary programming language |

### Libraries & Dependencies

| Library | Purpose | Version |
|---------|---------|---------|
| **BCrypt.Net-Next** | Password hashing | Latest |
| **QRCoder** | QR code generation | Latest |
| **Microsoft.EntityFrameworkCore.Sqlite** | SQLite provider for EF Core | 9.0.x |
| **Microsoft.AspNetCore.Authentication.JwtBearer** | JWT authentication (future API) | 9.0.x |
| **Bootstrap** | CSS framework | 5.3.x |
| **Bootstrap Icons** | Icon library | 1.11.x |

### Development Tools

- **IDE**: Visual Studio Code, Visual Studio 2022, JetBrains Rider
- **Version Control**: Git + GitHub
- **CI/CD**: GitHub Actions
- **Database Tools**: SQLite Browser, VS Code SQLite extension
- **API Testing**: Postman (for future API endpoints)

---

## Data Model

### Entity Relationship Diagram (ERD)

```
┌─────────────────────────┐
│      Organization       │
│─────────────────────────│
│ PK  Id                  │
│     Name                │
│     Description         │
│     Website             │
│     ContactEmail        │
│     CreatedAt           │
└─────────────────────────┘
            │
            │ 1
            │
            │ N
            ▼
┌─────────────────────────┐          ┌─────────────────────────┐
│         User            │          │         Event           │
│─────────────────────────│          │─────────────────────────│
│ PK  Id                  │          │ PK  Id                  │
│     Email               │◄─────────│ FK  OrganizerId         │
│     PasswordHash        │   N    1 │     Title               │
│     Name                │          │     Description         │
│     Role (Enum)         │          │     EventDate           │
│     ApprovalStatus      │          │     Location            │
│ FK  OrganizationId      │          │     Capacity            │
│     StudentId           │          │     TicketsIssued       │
│     PhoneNumber         │          │     Price               │
│     Program             │          │     ImageUrl            │
│     YearOfStudy         │          │     Category (Enum)     │
│     Position            │          │     ApprovalStatus      │
│     Department          │          │ FK  OrganizationId      │
│     CreatedAt           │          │     CreatedAt           │
└─────────────────────────┘          └─────────────────────────┘
        │           │                          │
        │ 1         │                          │ 1
        │           │                          │
        │ N         │ N                        │ N
        ▼           │                          ▼
┌─────────────────────────┐          ┌─────────────────────────┐
│       Ticket            │          │      SavedEvent         │
│─────────────────────────│          │─────────────────────────│
│ PK  Id                  │          │ PK  Id                  │
│ FK  UserId              │          │ FK  UserId              │
│ FK  EventId             │◄─────────│ FK  EventId             │
│     UniqueCode          │   N    1 │     SavedAt             │
│     QrCodeBase64        │          └─────────────────────────┘
│     ClaimedAt           │
│     IsRedeemed          │
│     RedeemedAt          │
└─────────────────────────┘


ENUMERATIONS:

UserRole:
  - Student (0)
  - Organizer (1)
  - Admin (2)

ApprovalStatus:
  - Pending (0)
  - Approved (1)
  - Rejected (2)

EventCategory:
  - Academic
  - Social
  - Career
  - Sports
  - Arts
  - Other
```

### Entity Descriptions

#### **User**
Represents all system users (students, organizers, administrators).

**Attributes:**
- `Id` (int, PK): Unique identifier
- `Email` (string, unique): Login credential
- `PasswordHash` (string): BCrypt-hashed password
- `Name` (string): Full name
- `Role` (UserRole enum): Access level
- `ApprovalStatus` (ApprovalStatus enum): Account approval state
- `OrganizationId` (int?, FK): Organization affiliation (organizers only)
- `StudentId` (string?): Student identification number
- `PhoneNumber` (string?): Contact number
- `Program` (string?): Academic program (students)
- `YearOfStudy` (string?): Year level (students)
- `Position` (string?): Role in organization (organizers)
- `Department` (string?): Department affiliation (organizers)
- `CreatedAt` (DateTime): Account creation timestamp

**Relationships:**
- 1:N with Ticket (user can have many tickets)
- 1:N with SavedEvent (user can save many events)
- N:1 with Organization (organizers belong to one organization)
- 1:N with Event (organizers create many events)

#### **Event**
Represents campus events that can be attended.

**Attributes:**
- `Id` (int, PK): Unique identifier
- `Title` (string): Event name
- `Description` (string): Detailed description
- `EventDate` (DateTime): Event date and time
- `Location` (string): Venue/location
- `Capacity` (int): Maximum attendees
- `TicketsIssued` (int): Current tickets distributed
- `Price` (decimal): Ticket price (0 for free)
- `ImageUrl` (string?): Event poster/image
- `Category` (EventCategory enum): Event type
- `ApprovalStatus` (ApprovalStatus enum): Moderation state
- `OrganizerId` (int, FK): Creator user ID
- `OrganizationId` (int?, FK): Host organization
- `CreatedAt` (DateTime): Creation timestamp

**Relationships:**
- N:1 with User (created by one organizer)
- N:1 with Organization (hosted by one organization)
- 1:N with Ticket (event has many tickets)
- 1:N with SavedEvent (saved by many students)

#### **Ticket**
Represents a student's claim/purchase of an event ticket.

**Attributes:**
- `Id` (int, PK): Unique identifier
- `UserId` (int, FK): Ticket owner
- `EventId` (int, FK): Associated event
- `UniqueCode` (string): Unique ticket identifier (GUID)
- `QrCodeBase64` (string): Base64-encoded QR code image
- `ClaimedAt` (DateTime): Ticket claim timestamp
- `IsRedeemed` (bool): Check-in status
- `RedeemedAt` (DateTime?): Check-in timestamp

**Relationships:**
- N:1 with User (belongs to one student)
- N:1 with Event (for one event)

#### **Organization**
Represents campus clubs, departments, or groups hosting events.

**Attributes:**
- `Id` (int, PK): Unique identifier
- `Name` (string): Organization name
- `Description` (string): About the organization
- `Website` (string?): Organization website
- `ContactEmail` (string): Contact email
- `CreatedAt` (DateTime): Registration timestamp

**Relationships:**
- 1:N with User (has many organizer members)
- 1:N with Event (hosts many events)

#### **SavedEvent**
Represents a student's saved/bookmarked event (personal calendar).

**Attributes:**
- `Id` (int, PK): Unique identifier
- `UserId` (int, FK): Student who saved
- `EventId` (int, FK): Saved event
- `SavedAt` (DateTime): Save timestamp

**Relationships:**
- N:1 with User (student's saved list)
- N:1 with Event (reference to event)

---

## Application Layers

### 1. Presentation Layer (Pages/)

**Responsibility**: Handle HTTP requests, render UI, user interaction

**Structure**:
```
Pages/
├── Shared/
│   ├── _Layout.cshtml              # Main layout
│   ├── _AdminLayout.cshtml         # Admin-specific layout
│   ├── _StudentLayout.cshtml       # Student-specific layout
│   └── _OrganizerLayout.cshtml     # Organizer-specific layout
│
├── Student/
│   ├── Home.cshtml / .cshtml.cs
│   ├── EventDetails.cshtml / .cshtml.cs
│   ├── Tickets.cshtml / .cshtml.cs
│   └── SavedEvents.cshtml / .cshtml.cs
│
├── Organizer/
│   ├── Home.cshtml / .cshtml.cs
│   ├── Events.cshtml / .cshtml.cs
│   ├── CreateEvent.cshtml / .cshtml.cs
│   └── EventDetails.cshtml / .cshtml.cs
│
├── Admin/
│   ├── Home.cshtml / .cshtml.cs
│   ├── Users.cshtml / .cshtml.cs
│   ├── Events.cshtml / .cshtml.cs
│   └── Organizations.cshtml / .cshtml.cs
│
├── Index.cshtml / .cshtml.cs        # Landing page
├── Login.cshtml / .cshtml.cs
├── SignupStudent.cshtml / .cshtml.cs
├── SignupOrganizer.cshtml / .cshtml.cs
└── Signup.cshtml / .cshtml.cs       # Role selection
```

**Key Patterns**:
- **Page Model Pattern**: Each .cshtml has a corresponding .cshtml.cs PageModel
- **Handler Methods**: OnGet, OnPost, OnPostClaimTicket, etc.
- **Model Binding**: Automatic binding of form data to properties
- **TempData**: Cross-request message passing (success/error notifications)

### 2. Business Logic Layer (Page Models + Services)

**Responsibility**: Application logic, validation, business rules

**Key Components**:

**Page Models** (`*.cshtml.cs`):
- Handle HTTP request processing
- Validate user input
- Orchestrate data access
- Apply business rules
- Manage session state

**Services** (`Data/DbCSVCommunicator.cs`):
- CSV export functionality
- Database seeding
- Reusable business operations

**Business Rules Examples**:
- Students can only claim tickets if event is approved and has capacity
- Organizers need admin approval before creating events
- Tickets generate unique QR codes on claim
- Events can't be edited after tickets are issued
- Admins cannot delete their own accounts

### 3. Data Access Layer (Data/)

**Responsibility**: Database interactions, query optimization

**Key Components**:

**AppDbContext** (`Data/AppDbContext.cs`):
```csharp
public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<SavedEvent> SavedEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Entity configurations
        // Indexes for performance
        // Relationships
    }
}
```

**Query Patterns**:
- **LINQ to Entities**: Type-safe queries
- **Eager Loading**: `.Include()` to prevent N+1 queries
- **Async Operations**: All database operations use async/await
- **Change Tracking**: Automatic dirty checking

**Example Query**:
```csharp
var events = await _context.Events
    .Include(e => e.Organization)
    .Where(e => e.ApprovalStatus == ApprovalStatus.Approved)
    .Where(e => e.EventDate > DateTime.UtcNow)
    .OrderBy(e => e.EventDate)
    .ToListAsync();
```

### 4. Domain Model Layer (Models/)

**Responsibility**: Core business entities, domain logic

**Key Models**:
- `User.cs`: User accounts and authentication
- `Event.cs`: Event information
- `Ticket.cs`: Ticketing and QR codes
- `Organization.cs`: Organization management
- `SavedEvent.cs`: Student calendar
- `UserRole.cs`: Role enumeration
- `ApprovalStatus.cs`: Approval state enumeration
- `EventCategory.cs`: Event categorization

---

## Security Architecture

### Authentication

**Session-Based Authentication**:
- Configured in `Program.cs`
- Session storage: In-memory (development) / Distributed cache (production)
- Session timeout: 20 minutes of inactivity
- Session data: UserId, UserRole

**Login Flow**:
```
1. User submits email + password
2. Server queries User table by email
3. BCrypt.Verify() checks password hash
4. On success: Session created with UserId
5. Redirect to role-specific dashboard
6. Subsequent requests validate session
```

**Session Configuration** (Program.cs):
```csharp
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
```

### Authorization

**Role-Based Access Control (RBAC)**:
- Three roles: Student, Organizer, Admin
- Page-level authorization in PageModel.OnGet()
- Redirect to login if no session
- Redirect to appropriate dashboard if wrong role

**Authorization Check Pattern**:
```csharp
public async Task<IActionResult> OnGetAsync()
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
    {
        return RedirectToPage("/Login");
    }

    var user = await _context.Users.FindAsync(userId.Value);
    if (user.Role != UserRole.Admin)
    {
        return RedirectToPage("/Index");
    }

    // Authorized - proceed
    return Page();
}
```

### Password Security

**BCrypt Hashing**:
- Library: BCrypt.Net-Next
- Work factor: 10 (default)
- Automatic salt generation
- One-way hashing (cannot decrypt)

**Implementation**:
```csharp
// Hashing on signup
var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

// Verification on login
bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
```

### Data Protection

**SQL Injection Prevention**:
- Entity Framework Core parameterizes all queries automatically
- No raw SQL queries in current implementation

**XSS Prevention**:
- Razor Pages automatically HTML-encodes output
- User input sanitized on render

**CSRF Protection**:
- ASP.NET Core anti-forgery tokens on all POST forms
- Automatically validated on submission

**QR Code Security**:
- Unique GUID per ticket (UniqueCode)
- Stored as Base64 in database
- Cannot be duplicated or forged
- Ticket invalidated after redemption

---

## Deployment Architecture

### Development Environment

```
┌──────────────────────────────────────┐
│    Developer Workstation             │
│  ┌────────────────────────────────┐ │
│  │   ASP.NET Core Dev Server      │ │
│  │   (Kestrel)                    │ │
│  │   Port: 5136 (HTTP)            │ │
│  │   Port: 7295 (HTTPS)           │ │
│  └────────────────────────────────┘ │
│              │                       │
│              ▼                       │
│  ┌────────────────────────────────┐ │
│  │  SQLite Database (File)        │ │
│  │  campusevents.db               │ │
│  └────────────────────────────────┘ │
└──────────────────────────────────────┘
```

### Production Environment (Proposed)

```
┌─────────────────────────────────────────────────┐
│              Cloud Provider (Azure/AWS)         │
│                                                 │
│  ┌───────────────────────────────────────────┐ │
│  │         Load Balancer / CDN               │ │
│  └───────────────────────────────────────────┘ │
│                      │                         │
│        ┌─────────────┴─────────────┐          │
│        ▼                           ▼          │
│  ┌─────────────┐            ┌─────────────┐  │
│  │  Web Server │            │  Web Server │  │
│  │  Instance 1 │            │  Instance 2 │  │
│  │  (Kestrel)  │            │  (Kestrel)  │  │
│  └─────────────┘            └─────────────┘  │
│        │                           │          │
│        └───────────┬───────────────┘          │
│                    ▼                          │
│         ┌─────────────────────┐              │
│         │  PostgreSQL / MySQL │              │
│         │  (Relational DB)    │              │
│         └─────────────────────┘              │
│                                               │
│  ┌───────────────────────────────────────┐  │
│  │     Blob Storage (Event Images)       │  │
│  └───────────────────────────────────────┘  │
└─────────────────────────────────────────────┘
```

### CI/CD Pipeline (GitHub Actions)

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│   Commit     │────>│    Build     │────>│     Test     │────>│    Deploy    │
│  to GitHub   │     │  dotnet      │     │   dotnet     │     │   to Cloud   │
│              │     │   build      │     │    test      │     │   Provider   │
└──────────────┘     └──────────────┘     └──────────────┘     └──────────────┘
```

**Workflow** (`.github/workflows/dotnet.yml`):
1. Checkout code
2. Setup .NET 9.0 SDK
3. Restore dependencies
4. Build solution
5. Run tests
6. (Future) Deploy to hosting

---

## Design Patterns

### 1. Model-View-PageModel (MVPM)
**Used in**: All Razor Pages
**Purpose**: Separation of concerns between UI, data, and logic

```
View (.cshtml)        ──> Presents UI to user
      │
      ▼
PageModel (.cshtml.cs) ──> Handles requests, business logic
      │
      ▼
Model (Entity)        ──> Represents domain data
```

### 2. Repository Pattern (via EF Core)
**Used in**: Data Access Layer
**Purpose**: Abstract data access, enable testing

```csharp
// DbContext acts as repository
var events = await _context.Events
    .Include(e => e.Organization)
    .ToListAsync();
```

### 3. Dependency Injection
**Used in**: Throughout application
**Purpose**: Loose coupling, testability

```csharp
// Service registration (Program.cs)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddTransient<DbCSVCommunicator>();

// Injection in PageModel
public class HomeModel : PageModel
{
    private readonly AppDbContext _context;

    public HomeModel(AppDbContext context)
    {
        _context = context;
    }
}
```

### 4. Factory Pattern
**Used in**: QR Code generation
**Purpose**: Object creation abstraction

```csharp
QRCodeGenerator qrGenerator = new QRCodeGenerator();
QRCodeData qrCodeData = qrGenerator.CreateQrCode(ticket.UniqueCode, QRCodeGenerator.ECCLevel.Q);
PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
byte[] qrCodeImage = qrCode.GetGraphic(20);
```

### 5. Unit of Work (via EF Core)
**Used in**: Database transactions
**Purpose**: Atomic operations

```csharp
// Multiple operations in one transaction
_context.Tickets.Add(newTicket);
_context.Events.Update(eventData);
await _context.SaveChangesAsync(); // Commits both or rolls back
```

### 6. Template Method Pattern
**Used in**: Shared layouts
**Purpose**: Reusable UI structure

```html
<!-- _Layout.cshtml defines structure -->
<body>
    <header>...</header>
    @RenderBody()  <!-- Child pages fill this -->
    <footer>...</footer>
</body>
```

---

## API Design

### Current State
The application currently uses **server-side rendering** with Razor Pages. No REST API exists yet.

### Future API Endpoints (Proposed)

**Authentication**:
- `POST /api/auth/login` - User login
- `POST /api/auth/logout` - User logout
- `POST /api/auth/register` - User registration

**Events**:
- `GET /api/events` - List events (with filters)
- `GET /api/events/{id}` - Get event details
- `POST /api/events` - Create event (organizer)
- `PUT /api/events/{id}` - Update event (organizer)
- `DELETE /api/events/{id}` - Delete event (organizer)

**Tickets**:
- `GET /api/tickets` - User's tickets
- `POST /api/tickets/claim/{eventId}` - Claim ticket
- `GET /api/tickets/{id}/qr` - Get QR code
- `POST /api/tickets/{id}/redeem` - Redeem ticket (check-in)

**Admin**:
- `GET /api/admin/users` - List users
- `PATCH /api/admin/users/{id}/approve` - Approve user
- `GET /api/admin/events/pending` - Pending events
- `PATCH /api/admin/events/{id}/approve` - Approve event

**Response Format**:
```json
{
  "success": true,
  "data": { ... },
  "message": "Operation successful",
  "errors": []
}
```

---

## Scalability Considerations

### Current Limitations (Prototype Phase)

1. **Database**: SQLite is single-file, not suitable for high concurrency
2. **Session Storage**: In-memory sessions don't scale across servers
3. **File Storage**: Event images stored in wwwroot (not scalable)
4. **No Caching**: Every request hits database
5. **Synchronous Operations**: Some blocking calls

### Future Improvements

#### 1. Database Migration
**Switch to PostgreSQL or MySQL**:
- Handle concurrent connections
- Better performance for large datasets
- ACID compliance at scale
- Replication support

#### 2. Distributed Caching
**Redis Implementation**:
```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "redis-server:6379";
});

// Cache frequently accessed data
await _cache.SetStringAsync("events:popular", serializedEvents,
    new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
    });
```

#### 3. Blob Storage for Images
**Azure Blob Storage / AWS S3**:
- Store event images externally
- CDN integration for fast delivery
- Reduce web server load

#### 4. Horizontal Scaling
**Load Balancer + Multiple Instances**:
```
     Load Balancer
    /      |      \
Server1  Server2  Server3
    \      |      /
      Database
```

#### 5. Asynchronous Processing
**Message Queue (RabbitMQ / Azure Service Bus)**:
- Email notifications (async)
- PDF ticket generation (async)
- Analytics aggregation (background job)

#### 6. API Rate Limiting
**Prevent abuse**:
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
        httpContext => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? "anonymous",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});
```

#### 7. Database Indexing
**Optimize queries**:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Event>()
        .HasIndex(e => e.EventDate);

    modelBuilder.Entity<Event>()
        .HasIndex(e => e.ApprovalStatus);

    modelBuilder.Entity<Ticket>()
        .HasIndex(t => t.UniqueCode)
        .IsUnique();
}
```

---

## Conclusion

The Campus Events & Ticketing System architecture is designed for **rapid development and middle-fidelity prototyping** while maintaining clear separation of concerns and extensibility. The layered monolithic approach with Razor Pages provides an appropriate balance of simplicity and structure for a student project.

### Key Strengths:
- ✅ Clear separation of concerns
- ✅ Role-based access control
- ✅ Secure authentication and authorization
- ✅ Maintainable codebase structure
- ✅ Room for future enhancements

### Future Evolution:
- Migrate to microservices for true scalability
- Add REST API for mobile app support
- Implement real-time features (WebSockets/SignalR)
- Advanced analytics with machine learning
- Integration with external services (payment, email, SMS)

---

**Document Version:** 1.0
**Last Updated:** November 3, 2025
**Team:** MONDAY_FK
**Course:** SOEN 341 - Software Process (Fall 2025)
