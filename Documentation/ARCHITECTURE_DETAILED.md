# Campus Events System - Detailed Architecture Documentation

## Table of Contents

1. [System Overview](#system-overview)
2. [Architecture Patterns](#architecture-patterns)
3. [Technology Stack](#technology-stack)
4. [Application Layers](#application-layers)
5. [Data Model](#data-model)
6. [Security Architecture](#security-architecture)
7. [Service Layer Design](#service-layer-design)
8. [Database Architecture](#database-architecture)
9. [Frontend Architecture](#frontend-architecture)
10. [Deployment Architecture](#deployment-architecture)
11. [Performance Considerations](#performance-considerations)
12. [Scalability Design](#scalability-design)

---

## System Overview

The Campus Events & Ticketing System is a comprehensive web application designed to facilitate event management, ticketing, carpool coordination, and room rental services for a university campus community. The system serves three distinct user roles: Students, Organizers, and Administrators.

### Core Functionality

1. **Event Management**: Creation, approval, and discovery of campus events
2. **Ticketing System**: Free and paid ticket distribution with QR code validation
3. **Carpool Coordination**: Driver registration and ride-sharing for events
4. **Room Rental**: Room management and booking system for organizers
5. **Notification System**: Real-time notifications for user actions
6. **Administrative Dashboard**: User and content moderation tools

### System Goals

- **Reliability**: High availability and data consistency
- **Security**: Protection of user data and prevention of fraud
- **Usability**: Intuitive interface for all user types
- **Scalability**: Support for growing user base and event volume
- **Maintainability**: Clean code structure and comprehensive documentation

---

## Architecture Patterns

### 1. Layered Architecture

The application follows a traditional layered architecture pattern:

```
┌─────────────────────────────────────┐
│      Presentation Layer              │
│   (Razor Pages, Views, CSS, JS)     │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│      Application Layer               │
│   (Page Models, Request Handlers)    │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│      Business Logic Layer            │
│   (Services, Helpers, Validators)   │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│      Data Access Layer               │
│   (Entity Framework, DbContext)     │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│      Database Layer                  │
│   (SQLite Database)                 │
└─────────────────────────────────────┘
```

### 2. Service-Oriented Architecture (SOA)

Business logic is encapsulated in service classes that provide reusable, testable functionality:

- **CarpoolService**: Manages driver registration and carpool operations
- **RoomRentalService**: Handles room creation and rental management
- **TicketSigningService**: Provides secure QR code generation and validation
- **NotificationService**: Manages user notifications
- **EncryptionService**: Handles sensitive data encryption
- **LicenseValidationService**: Validates driver license information

### 3. Repository Pattern (Implicit)

Entity Framework Core acts as an abstraction over the database, providing repository-like functionality through `DbSet<T>` collections in `AppDbContext`.

### 4. Dependency Injection

The application uses ASP.NET Core's built-in dependency injection container to manage service lifetimes:

- **Singleton**: Services with no state or shared state (TicketSigningService, EncryptionService)
- **Scoped**: Services tied to request lifecycle (CarpoolService, RoomRentalService, AppDbContext)
- **Transient**: Services created fresh each time (DbCSVCommunicator)

### 5. Model-View-PageModel (MVPM) Pattern

Razor Pages uses a PageModel class that combines controller and view model responsibilities:

- **PageModel**: Contains page logic, data access, and view model properties
- **View (.cshtml)**: Contains presentation markup and Razor syntax
- **Model**: Domain entities representing business concepts

---

## Technology Stack

### Backend Technologies

#### ASP.NET Core 9.0
- **Framework**: Modern, cross-platform web framework
- **Razor Pages**: Page-based MVC alternative for simpler page-focused scenarios
- **Entity Framework Core**: ORM for database operations
- **Session Management**: Server-side session storage
- **Dependency Injection**: Built-in IoC container

#### Database
- **SQLite**: Lightweight, file-based database
- **Entity Framework Core Migrations**: Database versioning and schema management
- **Connection String**: Configurable via appsettings.json

#### Security Libraries
- **BCrypt.Net**: Password hashing with adaptive work factor
- **System.Security.Cryptography**: HMAC-SHA256 for ticket signing
- **Custom EncryptionService**: AES encryption for sensitive data

#### QR Code Generation
- **QRCoder**: Library for generating QR code images
- **Custom TicketSigningService**: HMAC-signed tokens embedded in QR codes

### Frontend Technologies

#### HTML/CSS/JavaScript
- **Bootstrap 5**: Responsive CSS framework
- **jQuery**: DOM manipulation and AJAX (where needed)
- **Custom CSS**: Application-specific styling
- **Razor Syntax**: Server-side templating

#### UI Components
- **Bootstrap Components**: Cards, modals, forms, navigation
- **Custom Components**: Notification bell, QR scanner, event cards

### Development Tools

#### Testing
- **xUnit**: Unit testing framework
- **Moq**: Mocking framework for dependencies
- **Entity Framework In-Memory Database**: Testing data access

#### Build & Deployment
- **.NET CLI**: Command-line build tools
- **Git**: Version control
- **Entity Framework Migrations**: Database deployment

---

## Application Layers

### 1. Presentation Layer

**Location**: `Pages/` directory

**Responsibilities**:
- User interface rendering
- Form validation display
- User interaction handling
- Navigation and routing

**Components**:
- **Razor Pages (.cshtml)**: View templates with Razor syntax
- **Page Models (.cshtml.cs)**: Page logic and data binding
- **Layout Files**: Shared layouts for different user roles
- **Partial Views**: Reusable UI components

**Key Files**:
- `Pages/Index.cshtml`: Landing page
- `Pages/Student/Events.cshtml`: Student event browsing
- `Pages/Organizer/CreateEvent.cshtml`: Event creation form
- `Pages/Admin/Home.cshtml`: Admin dashboard

### 2. Application Layer

**Location**: `Pages/*.cshtml.cs` files

**Responsibilities**:
- Request handling and routing
- Input validation
- Authorization checks
- Service orchestration
- Response generation

**Pattern**:
```csharp
public class EventDetailsModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly CarpoolService _carpoolService;
    
    // Page properties (view model)
    public Event Event { get; set; }
    public List<CarpoolOffer> CarpoolOffers { get; set; }
    
    // HTTP handlers
    public async Task OnGetAsync(int id) { ... }
    public async Task<IActionResult> OnPostClaimTicketAsync(int id) { ... }
}
```

### 3. Business Logic Layer

**Location**: `Services/` directory

**Responsibilities**:
- Business rule enforcement
- Data validation
- Complex calculations
- Cross-cutting concerns (logging, caching)

**Service Categories**:

#### Domain Services
- `CarpoolService`: Carpool business logic
- `RoomRentalService`: Room rental business logic
- `NotificationService`: Notification management

#### Infrastructure Services
- `TicketSigningService`: Cryptographic operations
- `EncryptionService`: Data encryption/decryption
- `LicenseValidationService`: License validation

#### Helper Services
- `ValidationHelper`: Input validation utilities
- `DateTimeHelper`: Date/time formatting
- `FormatHelper`: Data formatting (currency, percentages)
- `PasswordHelper`: Password hashing utilities
- `EmailHelper`: Email validation
- `QueryHelper`: Database query extensions

### 4. Data Access Layer

**Location**: `Data/` directory

**Responsibilities**:
- Database connection management
- Entity configuration
- Relationship mapping
- Query execution

**Key Components**:

#### AppDbContext
```csharp
public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    // ... other entity sets
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Entity configurations
        // Relationship mappings
        // Index definitions
    }
}
```

#### Entity Framework Features Used
- **Code-First Migrations**: Database schema versioning
- **Lazy Loading**: Navigation property loading (where appropriate)
- **Eager Loading**: `.Include()` for related data
- **Query Filtering**: LINQ queries with predicates
- **Transaction Support**: Automatic transaction management

### 5. Domain Model Layer

**Location**: `Models/` directory

**Responsibilities**:
- Business entity definitions
- Business rule representation
- Relationship modeling

**Entity Categories**:

#### Core Entities
- `User`: User accounts and profiles
- `Event`: Campus events
- `Ticket`: Event tickets
- `Organization`: Event organizing groups

#### Carpool System Entities
- `Driver`: Driver profiles
- `CarpoolOffer`: Ride offers
- `CarpoolPassenger`: Passenger assignments

#### Room Rental Entities
- `Room`: Rentable rooms
- `RoomRental`: Rental bookings

#### Supporting Entities
- `SavedEvent`: User-event many-to-many relationship
- `Notification`: User notifications

---

## Data Model

### Entity Relationship Diagram (Conceptual)

```
User (1) ────< (N) Event (Organizer)
User (1) ────< (N) Ticket (Owner)
User (1) ────< (N) Driver (Profile)
User (1) ────< (N) RoomRental (Renter)
User (1) ────< (N) Notification (Recipient)

Event (1) ────< (N) Ticket
Event (1) ────< (N) CarpoolOffer
Event (N) >───< (N) User (SavedEvents)

Organization (1) ────< (N) Event
Organization (1) ────< (N) User (Organizers)

Driver (1) ────< (N) CarpoolOffer
CarpoolOffer (1) ────< (N) CarpoolPassenger
CarpoolPassenger (N) >─── (1) User (Passenger)

Room (1) ────< (N) RoomRental
Room (N) >─── (1) User (Organizer)
```

### Key Relationships

#### One-to-Many Relationships
- User → Events (Organizer)
- User → Tickets (Owner)
- User → Drivers (Profile)
- Event → Tickets
- Event → CarpoolOffers
- Driver → CarpoolOffers
- Room → RoomRentals
- Organization → Events

#### Many-to-Many Relationships
- User ↔ Event (SavedEvents join table)
- CarpoolOffer ↔ User (CarpoolPassenger join table)

#### Optional Relationships
- Event → Organization (nullable)
- User → Organization (nullable, for organizers)

### Entity Details

#### User Entity
- **Primary Key**: `Id` (int, auto-increment)
- **Unique Constraints**: `Email`
- **Indexes**: Email (unique)
- **Encrypted Fields**: None (passwords hashed, not encrypted)
- **Audit Fields**: `CreatedAt`

#### Event Entity
- **Primary Key**: `Id` (int, auto-increment)
- **Foreign Keys**: `OrganizerId` (required), `OrganizationId` (optional)
- **Enums**: `EventCategory`, `TicketType`, `ApprovalStatus`
- **Business Rules**: 
  - Must have future event date
  - Capacity must be positive
  - Price required if TicketType is Paid

#### Ticket Entity
- **Primary Key**: `Id` (int, auto-increment)
- **Unique Constraints**: `UniqueCode`
- **Foreign Keys**: `EventId`, `UserId`
- **Indexes**: UniqueCode (unique)
- **Business Rules**:
  - One ticket per user per event
  - Cannot exceed event capacity

#### Driver Entity
- **Primary Key**: `Id` (int, auto-increment)
- **Foreign Keys**: `UserId` (required)
- **Encrypted Fields**: `DriverLicenseNumber`, `LicensePlate`
- **Enums**: `VehicleType`, `DriverStatus`, `DriverType`
- **Business Rules**:
  - Capacity: 1-50
  - Requires admin approval

#### Room Entity
- **Primary Key**: `Id` (int, auto-increment)
- **Foreign Keys**: `OrganizerId` (required)
- **Enums**: `RoomStatus`
- **Business Rules**:
  - Capacity must be positive
  - Availability window validation

#### RoomRental Entity
- **Primary Key**: `Id` (int, auto-increment)
- **Foreign Keys**: `RoomId`, `RenterId`
- **Enums**: `RentalStatus`
- **Indexes**: Composite index on (RoomId, StartTime, EndTime) for overlap detection
- **Business Rules**:
  - No overlapping rentals for same room
  - Start time must be in future
  - End time must be after start time

---

## Security Architecture

### Authentication

#### Session-Based Authentication
- **Storage**: Server-side session storage (DistributedMemoryCache)
- **Timeout**: 30 minutes idle timeout
- **Cookie Settings**: HttpOnly, Essential, Secure (in production)
- **Session Data**: User ID, User Role

#### Password Security
- **Hashing Algorithm**: BCrypt with work factor 12
- **Storage**: Hashed passwords only, never plain text
- **Validation**: Complexity requirements enforced

### Authorization

#### Role-Based Access Control (RBAC)
- **Roles**: Student, Organizer, Admin
- **Enforcement**: Page-level and action-level checks
- **Pattern**: `if (user.Role != UserRole.Admin) return Forbid()`

#### Resource-Level Authorization
- Users can only access their own resources
- Organizers can only manage their own events/rooms
- Admins have override capabilities

### Data Protection

#### Encryption at Rest
- **Driver License Numbers**: AES-256 encryption via EncryptionService
- **License Plates**: AES-256 encryption via EncryptionService
- **Encryption Key**: Stored in configuration (environment variables in production)

#### Encryption in Transit
- **HTTPS**: Enforced in production
- **HSTS**: HTTP Strict Transport Security headers

### Ticket Security

#### QR Code Signing
- **Algorithm**: HMAC-SHA256
- **Key Management**: Configuration-based (environment variables in production)
- **Token Structure**: 
  ```json
  {
    "payload": "base64_encoded_json",
    "signature": "base64_encoded_hmac"
  }
  ```
- **Payload Contents**: EventId, TicketId, UniqueCode, Expiry
- **Expiry**: 24 hours after event date
- **Verification**: Constant-time comparison to prevent timing attacks

### Input Validation

#### Client-Side Validation
- HTML5 form validation
- JavaScript validation (where applicable)
- Bootstrap validation classes

#### Server-Side Validation
- Model validation attributes
- Custom validation in services
- SQL injection prevention via parameterized queries

### Security Headers (Recommended for Production)

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'");
    await next();
});
```

---

## Service Layer Design

### Service Responsibilities

Each service class encapsulates a specific domain of functionality:

#### CarpoolService
- **Purpose**: Manage carpool system operations
- **Key Methods**:
  - `RegisterDriverAsync()`: Create driver profile
  - `CreateOfferAsync()`: Create carpool offer
  - `JoinOfferAsync()`: Add passenger to offer
  - `CancelOfferAsync()`: Cancel carpool offer
  - `GetUserCarpoolsAsync()`: Get user's carpools

#### RoomRentalService
- **Purpose**: Manage room rental operations
- **Key Methods**:
  - `CreateRoomAsync()`: Create rentable room
  - `RequestRentalAsync()`: Request room rental
  - `ApproveRentalAsync()`: Approve rental request
  - `GetAvailableRoomsAsync()`: Find available rooms
  - `DisableRoomAsync()`: Admin disable room

#### TicketSigningService
- **Purpose**: Generate and validate secure ticket tokens
- **Key Methods**:
  - `SignTicket()`: Create signed token for QR code
  - `VerifyTicket()`: Validate token signature and expiry
- **Lifetime**: Singleton (stateless, thread-safe)

#### NotificationService
- **Purpose**: Manage user notifications
- **Key Methods**:
  - `CreateNotificationAsync()`: Create notification
  - `GetUnreadNotificationsAsync()`: Get unread notifications
  - `MarkAsReadAsync()`: Mark notification as read
- **Integration**: Used by other services to send notifications

#### EncryptionService
- **Purpose**: Encrypt/decrypt sensitive data
- **Key Methods**:
  - `Encrypt()`: Encrypt string data
  - `Decrypt()`: Decrypt string data
- **Algorithm**: AES-256
- **Lifetime**: Singleton

### Service Communication Patterns

#### Direct Service Calls
Services can depend on other services via constructor injection:
```csharp
public class RoomRentalService
{
    private readonly NotificationService _notificationService;
    
    public RoomRentalService(
        AppDbContext context, 
        NotificationService notificationService)
    {
        _notificationService = notificationService;
    }
}
```

#### Event-Driven Notifications
Services trigger notifications after state changes:
```csharp
await _notificationService.NotifyRentalApprovedAsync(rentalId);
```

### Error Handling in Services

Services return tuples for operation results:
```csharp
public async Task<(bool Success, string Message, CarpoolOffer? Offer)> 
    CreateOfferAsync(...)
{
    // Validation
    if (driver.Status != DriverStatus.Active)
        return (false, "Driver not active", null);
    
    // Success
    return (true, "Offer created", offer);
}
```

---

## Database Architecture

### Database Technology

#### SQLite
- **Type**: File-based relational database
- **File Location**: `campusevents.db` (project root)
- **Advantages**: 
  - Zero configuration
  - Portable database file
  - Good for development and small deployments
- **Limitations**:
  - Single-writer concurrency
  - Not ideal for high-traffic production

### Schema Management

#### Entity Framework Migrations
- **Location**: `Migrations/` directory
- **Naming**: `{Timestamp}_{Description}.cs`
- **Process**:
  1. Modify entity models
  2. Create migration: `dotnet ef migrations add MigrationName`
  3. Review migration code
  4. Apply migration: `dotnet ef database update`

#### Migration History
- `20251003143905_InitialCreate.cs`: Initial schema
- `20251025212308_UpdateEventAndTicketModels.cs`: Event/Ticket updates
- `20251025233523_AddExtendedUserFields.cs`: User field extensions
- `20251117165744_AddCarpoolAndRoomRentalSystem.cs`: US.04 features
- `20251122022931_RemoveQrCodeImageColumn.cs`: QR code refactoring
- `20251124085746_AddDriverLicenseAndProvinceFields.cs`: Driver license fields
- `20251124165942_AllowMultipleDriversPerUser.cs`: Multiple drivers support
- `20251124173238_ConvertCategoryToEnum.cs`: Category enum conversion
- `20251124180314_AddNotificationSystem.cs`: Notification system

### Database Configuration

#### Connection String
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=campusevents.db"
  }
}
```

#### Context Configuration
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=campusevents.db"
    )
);
```

### Indexing Strategy

#### Primary Keys
- All entities have `Id` as auto-increment primary key

#### Unique Indexes
- `User.Email`: Unique email constraint
- `Ticket.UniqueCode`: Unique ticket code

#### Composite Indexes
- `RoomRental(RoomId, StartTime, EndTime)`: For overlap detection queries
- `Notification(UserId, IsRead, CreatedAt)`: For efficient notification queries

#### Foreign Key Indexes
- Entity Framework automatically indexes foreign keys

### Query Optimization

#### Eager Loading
```csharp
var events = await _context.Events
    .Include(e => e.Organizer)
    .Include(e => e.Organization)
    .Include(e => e.Tickets)
    .ToListAsync();
```

#### Filtered Includes
```csharp
var room = await _context.Rooms
    .Include(r => r.Rentals
        .Where(rental => rental.Status == RentalStatus.Approved))
    .FirstOrDefaultAsync(r => r.Id == roomId);
```

#### Projection Queries
```csharp
var eventSummaries = await _context.Events
    .Select(e => new {
        e.Id,
        e.Title,
        TicketCount = e.Tickets.Count
    })
    .ToListAsync();
```

---

## Frontend Architecture

### Razor Pages Structure

#### Page Organization
```
Pages/
├── Index.cshtml              # Landing page
├── Login.cshtml              # Authentication
├── Signup*.cshtml            # Registration pages
├── Student/                  # Student pages
│   ├── Events.cshtml
│   ├── Tickets.cshtml
│   ├── Carpools.cshtml
│   └── Rooms.cshtml
├── Organizer/                # Organizer pages
│   ├── CreateEvent.cshtml
│   ├── Events.cshtml
│   └── Rooms.cshtml
└── Admin/                    # Admin pages
    ├── Home.cshtml
    ├── Users.cshtml
    └── Events.cshtml
```

#### Layout System
- `_Layout.cshtml`: Default layout
- `_StudentLayout.cshtml`: Student-specific layout
- `_OrganizerLayout.cshtml`: Organizer-specific layout
- `_AdminLayout.cshtml`: Admin-specific layout

#### Partial Views
- `_NotificationBell.cshtml`: Notification dropdown
- `_ValidationScriptsPartial.cshtml`: Client-side validation scripts

### Client-Side Technologies

#### Bootstrap 5
- **Grid System**: Responsive layouts
- **Components**: Cards, modals, forms, navigation
- **Utilities**: Spacing, colors, typography

#### JavaScript
- **jQuery**: DOM manipulation (where needed)
- **Custom Scripts**: Application-specific functionality
- **AJAX**: Asynchronous requests for notifications

#### CSS Architecture
- **Bootstrap**: Base styles and components
- **Custom CSS**: Application-specific styling
- **Location**: `wwwroot/css/`

### State Management

#### Server-Side State
- **Session Storage**: User authentication, preferences
- **Database**: Persistent data

#### Client-Side State
- **Form State**: HTML form elements
- **JavaScript Variables**: Temporary UI state
- **No Client-Side Framework**: Vanilla JavaScript/jQuery

---

## Deployment Architecture

### Development Environment

#### Local Development
- **Database**: SQLite file in project directory
- **Server**: Kestrel web server
- **Ports**: HTTP (5136), HTTPS (7295)
- **Hot Reload**: `dotnet watch run`

#### Configuration
- **appsettings.Development.json**: Development settings
- **User Secrets**: Sensitive development data
- **Environment Variables**: Optional overrides

### Production Environment (Recommended)

#### Web Server
- **IIS**: Windows Server
- **Kestrel**: Cross-platform
- **Nginx**: Reverse proxy (Linux)

#### Database
- **SQL Server**: Recommended for production
- **PostgreSQL**: Alternative option
- **Migration**: Update connection string and run migrations

#### Configuration
- **appsettings.Production.json**: Production settings
- **Environment Variables**: Security keys, connection strings
- **Azure Key Vault**: Recommended for secrets management

#### Security
- **HTTPS**: Required
- **HSTS**: Enable
- **Security Headers**: Configure
- **Rate Limiting**: Implement
- **Logging**: Application Insights or similar

---

## Performance Considerations

### Database Performance

#### Query Optimization
- Use `.Include()` judiciously to avoid N+1 queries
- Use projection queries for read-only operations
- Add indexes for frequently queried columns
- Use `.AsNoTracking()` for read-only queries

#### Caching Strategy
- **In-Memory Cache**: For frequently accessed, rarely changed data
- **CacheHelper**: Utility for caching operations
- **Cache Invalidation**: Manual invalidation on updates

### Application Performance

#### Async/Await
- All database operations are asynchronous
- All service methods return `Task` or `Task<T>`
- Prevents thread pool exhaustion

#### Connection Pooling
- Entity Framework manages connection pooling automatically
- SQLite has limited concurrency (single writer)

#### Session Management
- DistributedMemoryCache for development
- Redis or SQL Server for production scale-out

### Frontend Performance

#### Asset Optimization
- Minify CSS and JavaScript in production
- Enable gzip compression
- Use CDN for Bootstrap/jQuery (optional)

#### Lazy Loading
- Load notifications asynchronously
- Paginate event lists
- Defer non-critical JavaScript

---

## Scalability Design

### Current Limitations

#### SQLite Constraints
- Single-writer concurrency
- File-based (not network-accessible)
- Limited to moderate traffic

### Scaling Strategies

#### Vertical Scaling
- Increase server resources (CPU, RAM)
- Upgrade to SQL Server or PostgreSQL
- Optimize database queries

#### Horizontal Scaling
- **Load Balancing**: Multiple web server instances
- **Session State**: Redis or SQL Server session store
- **Database**: Read replicas for reporting queries
- **CDN**: Static asset delivery

#### Microservices Migration (Future)
- Separate services for:
  - Event Management
  - Ticketing
  - Carpool System
  - Room Rental
  - Notifications
- API Gateway for routing
- Message queue for async operations

### Database Migration Path

#### From SQLite to SQL Server
1. Export data from SQLite
2. Update connection string
3. Run migrations on SQL Server
4. Import data
5. Update Entity Framework provider

---

## Conclusion

This architecture provides a solid foundation for the Campus Events system while maintaining flexibility for future enhancements. The layered design promotes separation of concerns, testability, and maintainability.

**Key Strengths**:
- Clear separation of concerns
- Comprehensive security measures
- Scalable service design
- Well-documented data model

**Areas for Future Enhancement**:
- API layer for mobile applications
- Real-time notifications (SignalR)
- Advanced analytics and reporting
- Integration with external services (payment gateways, email services)

---

**Document Version**: 1.0
**Last Updated**: 2025-01-XX
**Maintained By**: Development Team

