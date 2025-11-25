# Campus Events System - Services Documentation

## Overview

This document provides comprehensive documentation for all service classes in the Campus Events system. Services encapsulate business logic and provide reusable functionality across the application.

## Table of Contents

1. [Core Services](#core-services)
2. [Security Services](#security-services)
3. [Business Logic Services](#business-logic-services)
4. [Helper Services](#helper-services)
5. [Service Patterns](#service-patterns)
6. [Dependency Injection](#dependency-injection)

---

## Core Services

### CarpoolService

**Location**: `Services/CarpoolService.cs`

**Purpose**: Manages the carpool system, including driver registration, ride offers, and passenger management.

**Dependencies**:
- `AppDbContext`: Database access

**Key Methods**:

#### RegisterDriverAsync
```csharp
Task<(bool Success, string Message, Driver? Driver)> RegisterDriverAsync(
    int userId,
    int capacity,
    VehicleType vehicleType,
    DriverType driverType,
    string? licensePlate = null,
    string accessibilityFeatures = "")
```

**Purpose**: Register a user as a driver for the carpool system.

**Parameters**:
- `userId`: ID of user registering as driver
- `capacity`: Passenger capacity (1-50)
- `vehicleType`: Type of vehicle (Mini, Sedan, SUV, Van, Bus)
- `driverType`: Student or Organizer driver
- `licensePlate`: Optional license plate (encrypted)
- `accessibilityFeatures`: Comma-separated accessibility features

**Returns**: Tuple with success status, message, and driver entity if successful.

**Business Rules**:
- User must exist
- User cannot already be a driver
- Driver type must match user role
- Capacity must be between 1 and 50
- Driver status set to Pending (requires admin approval)

**Error Cases**:
- User not found
- User already registered as driver
- Invalid driver type for user role
- Invalid capacity range

---

#### CreateOfferAsync
```csharp
Task<(bool Success, string Message, CarpoolOffer? Offer)> CreateOfferAsync(
    int driverId,
    int eventId,
    string departureInfo,
    DateTime departureTime,
    string? departureAddress = null,
    double? latitude = null,
    double? longitude = null)
```

**Purpose**: Create a carpool offer for an event.

**Parameters**:
- `driverId`: ID of driver creating offer
- `eventId`: ID of event for carpool
- `departureInfo`: Departure details (time, location)
- `departureTime`: Estimated departure time
- `departureAddress`: Optional physical address
- `latitude`: Optional latitude for proximity
- `longitude`: Optional longitude for proximity

**Returns**: Tuple with success status, message, and offer entity if successful.

**Business Rules**:
- Driver must exist and be active
- Event must exist
- Driver cannot have duplicate active offers for same event
- SeatsAvailable initialized from driver capacity

**Error Cases**:
- Driver not found
- Driver not active
- Event not found
- Duplicate offer exists

---

#### JoinOfferAsync
```csharp
Task<(bool Success, string Message)> JoinOfferAsync(
    int offerId,
    int passengerId,
    string? pickupLocation = null,
    string? notes = null)
```

**Purpose**: Add a passenger to a carpool offer.

**Parameters**:
- `offerId`: ID of carpool offer
- `passengerId`: ID of user joining as passenger
- `pickupLocation`: Optional pickup location
- `notes`: Optional notes for driver

**Returns**: Tuple with success status and message.

**Business Rules**:
- Offer must exist and be active
- Seats must be available
- Passenger cannot be the driver
- Passenger cannot already be joined
- SeatsAvailable decremented on success
- Offer status set to Full if no seats remain

**Error Cases**:
- Offer not found
- Offer not active
- No seats available
- Passenger is driver
- Passenger already joined

---

#### GetUserCarpoolsAsync
```csharp
Task<(List<CarpoolOffer> AsDriver, List<CarpoolPassenger> AsPassenger)> GetUserCarpoolsAsync(int userId)
```

**Purpose**: Get all carpool participations for a user (as driver and passenger).

**Parameters**:
- `userId`: ID of user

**Returns**: Tuple with lists of offers as driver and passengers as passenger.

**Includes**:
- Event information
- Driver/passenger details
- Passenger lists for driver offers

---

### RoomRentalService

**Location**: `Services/RoomRentalService.cs`

**Purpose**: Manages room rental system, including room creation, rental requests, and availability checking.

**Dependencies**:
- `AppDbContext`: Database access
- `NotificationService`: Optional, for sending notifications

**Key Methods**:

#### CreateRoomAsync
```csharp
Task<(bool Success, string Message, Room? Room)> CreateRoomAsync(
    int organizerId,
    string name,
    string address,
    int capacity,
    string? roomInfo = null,
    string amenities = "",
    decimal? hourlyRate = null,
    DateTime? availabilityStart = null,
    DateTime? availabilityEnd = null)
```

**Purpose**: Create a new rentable room.

**Parameters**:
- `organizerId`: ID of organizer creating room
- `name`: Room name/identifier
- `address`: Physical address
- `capacity`: Maximum capacity
- `roomInfo`: Optional detailed information
- `amenities`: Comma-separated amenities
- `hourlyRate`: Optional hourly rental rate
- `availabilityStart`: Optional availability window start
- `availabilityEnd`: Optional availability window end

**Returns**: Tuple with success status, message, and room entity if successful.

**Business Rules**:
- User must be organizer
- Capacity must be positive
- AvailabilityEnd must be after AvailabilityStart if both provided
- Room status set to Enabled by default

**Error Cases**:
- User not found
- User not organizer
- Invalid capacity
- Invalid availability window

---

#### RequestRentalAsync
```csharp
Task<(bool Success, string Message, RoomRental? Rental)> RequestRentalAsync(
    int roomId,
    int renterId,
    DateTime startTime,
    DateTime endTime,
    string? purpose = null,
    int? expectedAttendees = null)
```

**Purpose**: Request a room rental.

**Parameters**:
- `roomId`: ID of room to rent
- `renterId`: ID of user requesting rental
- `startTime`: Rental start time
- `endTime`: Rental end time
- `purpose`: Optional rental purpose
- `expectedAttendees`: Optional expected number of attendees

**Returns**: Tuple with success status, message, and rental entity if successful.

**Business Rules**:
- Room must exist and be enabled
- Start time must be in future
- End time must be after start time
- Must not overlap with existing Approved/Pending rentals
- Expected attendees cannot exceed room capacity
- Must be within room availability window if specified
- TotalCost calculated from hourly rate and duration
- Rental status set to Pending (requires approval)

**Error Cases**:
- Room not found
- Room disabled
- Invalid time range
- Time in past
- Overlapping rental exists
- Capacity exceeded
- Outside availability window

---

#### ApproveRentalAsync
```csharp
Task<(bool Success, string Message)> ApproveRentalAsync(
    int rentalId,
    int approverId,
    bool isAdmin = false)
```

**Purpose**: Approve a rental request.

**Parameters**:
- `rentalId`: ID of rental to approve
- `approverId`: ID of user approving (organizer or admin)
- `isAdmin`: Whether approver is admin

**Returns**: Tuple with success status and message.

**Business Rules**:
- Approver must be room organizer or admin
- Room must still be enabled
- Re-checks for overlaps (race condition protection)
- Rental status set to Approved
- Notification sent to renter

**Error Cases**:
- Rental not found
- Unauthorized approver
- Room disabled
- Conflicting rental already approved

---

#### GetAvailableRoomsAsync
```csharp
Task<List<Room>> GetAvailableRoomsAsync(
    DateTime startTime,
    DateTime endTime,
    int? minCapacity = null)
```

**Purpose**: Find rooms available for a time slot.

**Parameters**:
- `startTime`: Rental start time
- `endTime`: Rental end time
- `minCapacity`: Optional minimum capacity requirement

**Returns**: List of available rooms.

**Business Logic**:
- Filters enabled rooms
- Checks capacity requirements
- Validates availability windows
- Excludes rooms with overlapping Approved/Pending rentals

---

### TicketSigningService

**Location**: `Services/TicketSigningService.cs`

**Purpose**: Provides HMAC-based signing and verification for ticket QR codes.

**Lifetime**: Singleton (stateless, thread-safe)

**Dependencies**:
- `IConfiguration`: For signing key

**Key Methods**:

#### SignTicket
```csharp
string SignTicket(int eventId, int ticketId, string uniqueCode, DateTime eventDate)
```

**Purpose**: Create a signed token for QR code generation.

**Parameters**:
- `eventId`: Event ID
- `ticketId`: Ticket ID
- `uniqueCode`: Unique ticket code
- `eventDate`: Event date for expiry calculation

**Returns**: JSON-encoded signed token.

**Token Structure**:
```json
{
  "payload": "base64_encoded_json",
  "signature": "base64_encoded_hmac"
}
```

**Payload Contents**:
- Version: Token version (1)
- EventId: Event ID
- TicketId: Ticket ID
- UniqueCode: Unique ticket code
- Expiry: Expiry timestamp (24 hours after event)

**Security**:
- HMAC-SHA256 signature prevents forgery
- Expiry timestamp prevents reuse after event
- Constant-time comparison for verification

---

#### VerifyTicket
```csharp
TicketValidationResult? VerifyTicket(string signedToken)
```

**Purpose**: Verify a signed ticket token.

**Parameters**:
- `signedToken`: JSON-encoded signed token

**Returns**: Validation result with payload if valid, null if invalid.

**Validation Steps**:
1. Parse signed token JSON
2. Decode payload and signature
3. Verify HMAC signature (constant-time)
4. Parse payload JSON
5. Check token version
6. Check expiry timestamp

**Error Cases**:
- Malformed token
- Invalid signature (forgery/tampering)
- Unsupported version
- Expired token

---

### EncryptionService

**Location**: `Services/EncryptionService.cs`

**Purpose**: Encrypts and decrypts sensitive data using AES-256.

**Lifetime**: Singleton

**Dependencies**:
- `IConfiguration`: For encryption key

**Key Methods**:

#### Encrypt
```csharp
string Encrypt(string plainText)
```

**Purpose**: Encrypt a plaintext string.

**Parameters**:
- `plainText`: String to encrypt

**Returns**: Base64-encoded encrypted string.

**Algorithm**: AES-256-CBC with PKCS7 padding

**Key Management**:
- Key derived from configuration using SHA256
- IV derived from key + "_iv"
- Key must be at least 32 characters

---

#### Decrypt
```csharp
string Decrypt(string encryptedText)
```

**Purpose**: Decrypt an encrypted string.

**Parameters**:
- `encryptedText`: Base64-encoded encrypted string

**Returns**: Decrypted plaintext string.

**Error Handling**:
- Returns empty string if decryption fails
- Handles corrupted data gracefully

---

#### EncryptLicensePlate / DecryptLicensePlate
```csharp
string EncryptLicensePlate(string licensePlate)
string DecryptLicensePlate(string encryptedPlate)
```

**Purpose**: Encrypt/decrypt license plates with uppercase normalization.

---

#### EncryptLicenseNumber / DecryptLicenseNumber
```csharp
string EncryptLicenseNumber(string licenseNumber)
string DecryptLicenseNumber(string encryptedNumber)
```

**Purpose**: Encrypt/decrypt driver license numbers with uppercase normalization.

---

### NotificationService

**Location**: `Services/NotificationService.cs`

**Purpose**: Manages user notifications for system events.

**Dependencies**:
- `AppDbContext`: Database access

**Key Methods**:

#### CreateNotificationAsync
```csharp
Task<Notification> CreateNotificationAsync(
    int userId,
    NotificationType type,
    string title,
    string message,
    int? relatedEntityId = null,
    string? relatedEntityType = null,
    string? actionUrl = null)
```

**Purpose**: Create a notification for a user.

**Parameters**:
- `userId`: User to notify
- `type`: Notification type
- `title`: Notification title (max 200 chars)
- `message`: Notification message (max 1000 chars)
- `relatedEntityId`: Optional related entity ID
- `relatedEntityType`: Optional related entity type
- `actionUrl`: Optional action URL

**Returns**: Created notification entity.

---

#### GetUnreadNotificationsAsync
```csharp
Task<List<Notification>> GetUnreadNotificationsAsync(int userId)
```

**Purpose**: Get unread notifications for a user.

**Parameters**:
- `userId`: User ID

**Returns**: List of unread notifications, ordered by creation date (newest first).

---

#### MarkAsReadAsync
```csharp
Task<bool> MarkAsReadAsync(int notificationId, int userId)
```

**Purpose**: Mark a notification as read.

**Parameters**:
- `notificationId`: Notification ID
- `userId`: User ID (for authorization)

**Returns**: True if marked as read, false if not found.

**Authorization**: User must own the notification.

---

#### MarkAllAsReadAsync
```csharp
Task MarkAllAsReadAsync(int userId)
```

**Purpose**: Mark all user's unread notifications as read.

**Parameters**:
- `userId`: User ID

---

#### DeleteNotificationAsync
```csharp
Task<bool> DeleteNotificationAsync(int notificationId, int userId)
```

**Purpose**: Delete a notification.

**Parameters**:
- `notificationId`: Notification ID
- `userId`: User ID (for authorization)

**Returns**: True if deleted, false if not found.

**Authorization**: User must own the notification.

---

## Helper Services

### ValidationHelper

**Location**: `Services/ValidationHelper.cs`

**Purpose**: Provides common validation methods for user inputs.

**Type**: Static class

**Key Methods**:

- `IsValidEmail(string?)`: Validates email format
- `IsValidStudentId(string?)`: Validates 9-digit student ID
- `IsValidString(string?, int, int?)`: Validates string length
- `IsValidDateRange(DateTime, DateTime)`: Validates date range
- `IsFutureDate(DateTime)`: Checks if date is in future
- `IsValidCapacity(int, int, int)`: Validates capacity value
- `IsValidLatitude(double)`: Validates latitude (-90 to 90)
- `IsValidLongitude(double)`: Validates longitude (-180 to 180)
- `IsValidCoordinates(double?, double?)`: Validates both coordinates
- `SanitizeInput(string?)`: Removes HTML tags and control characters

---

### DateTimeHelper

**Location**: `Services/DateTimeHelper.cs`

**Purpose**: Provides date and time formatting utilities.

**Type**: Static class

**Key Methods**:

- `FormatForDisplay(DateTime, bool)`: Formats date for display
- `FormatRelativeTime(DateTime)`: Formats relative time ("2 hours ago")
- `FormatDuration(DateTime, DateTime)`: Formats duration between dates
- `IsWithinRange(DateTime, DateTime, DateTime)`: Checks if date in range
- `StartOfDay(DateTime)`: Gets start of day (00:00:00)
- `EndOfDay(DateTime)`: Gets end of day (23:59:59)
- `ToLocalTime(DateTime, string)`: Converts UTC to local time

---

### PasswordHelper

**Location**: `Services/PasswordHelper.cs`

**Purpose**: Provides password hashing and validation utilities.

**Type**: Static class

**Key Methods**:

- `HashPassword(string)`: Hashes password using BCrypt
- `VerifyPassword(string, string)`: Verifies password against hash
- `ValidatePasswordStrength(string?)`: Validates password strength
- `GenerateRandomPassword(int)`: Generates random secure password
- `NeedsRehash(string)`: Checks if password needs rehashing

---

### FormatHelper

**Location**: `Services/FormatHelper.cs`

**Purpose**: Provides data formatting utilities.

**Type**: Static class

**Key Methods**:

- `FormatCurrency(decimal, string)`: Formats currency amount
- `FormatPercentage(double, int)`: Formats percentage
- `FormatFileSize(long)`: Formats file size (B, KB, MB, GB, TB)
- `FormatPhoneNumber(string?)`: Formats phone number
- `FormatStudentId(string?)`: Formats student ID
- `FormatCapacity(int, int)`: Formats capacity (current/max)
- `FormatCapacityPercentage(int, int)`: Formats capacity as percentage
- `FormatList(IEnumerable<string>, int?)`: Formats list of items
- `FormatOrdinal(int)`: Formats ordinal number (1st, 2nd, 3rd)
- `FormatTimeSpan(TimeSpan)`: Formats time span

---

### LicenseValidationService

**Location**: `Services/LicenseValidationService.cs`

**Purpose**: Validates driver license numbers by province.

**Lifetime**: Singleton

**Key Methods**:

- `ValidateLicense(string, string)`: Validates license number for province
- `GetProvinceCodes()`: Gets list of valid province codes

**Supported Provinces**: ON, QC, BC, AB, SK, MB, NS, NB, PE, NL, YT, NT, NU

---

### ProximityService

**Location**: `Services/ProximityService.cs`

**Purpose**: Calculates geographic proximity for carpool offers.

**Dependencies**:
- `AppDbContext`: Database access

**Key Methods**:

- `FindNearbyOffers(double, double, double, int?)`: Finds carpool offers within radius
- `CalculateDistance(double, double, double, double)`: Calculates distance between coordinates

**Algorithm**: Haversine formula for great-circle distance

---

## Service Patterns

### Result Tuple Pattern

Services return tuples for operation results:

```csharp
Task<(bool Success, string Message, T? Result)> OperationAsync(...)
```

**Benefits**:
- Clear success/failure indication
- Descriptive error messages
- Optional result entity
- No exceptions for business logic errors

**Usage**:
```csharp
var (success, message, offer) = await _carpoolService.CreateOfferAsync(...);
if (!success)
{
    // Handle error with message
    return BadRequest(message);
}
// Use offer entity
```

---

### Dependency Injection Pattern

Services receive dependencies via constructor injection:

```csharp
public class CarpoolService
{
    private readonly AppDbContext _context;
    
    public CarpoolService(AppDbContext context)
    {
        _context = context;
    }
}
```

**Benefits**:
- Loose coupling
- Testability (can inject mocks)
- Clear dependencies
- Managed lifetimes

---

### Async/Await Pattern

All service methods are asynchronous:

```csharp
public async Task<T> MethodAsync(...)
{
    await _context.SaveChangesAsync();
    return result;
}
```

**Benefits**:
- Non-blocking I/O
- Scalability
- Thread pool efficiency

---

## Dependency Injection

### Service Registration

Services are registered in `Program.cs`:

```csharp
// Singleton services (stateless, thread-safe)
builder.Services.AddSingleton<TicketSigningService>();
builder.Services.AddSingleton<EncryptionService>();

// Scoped services (per-request)
builder.Services.AddScoped<CarpoolService>();
builder.Services.AddScoped<RoomRentalService>();
builder.Services.AddScoped<NotificationService>();

// Transient services (new instance each time)
builder.Services.AddTransient<DbCSVCommunicator>();
```

### Service Lifetimes

#### Singleton
- **Use**: Stateless services, shared configuration
- **Examples**: TicketSigningService, EncryptionService
- **Lifetime**: Single instance for application lifetime

#### Scoped
- **Use**: Services with per-request state, database access
- **Examples**: CarpoolService, RoomRentalService, AppDbContext
- **Lifetime**: One instance per HTTP request

#### Transient
- **Use**: Lightweight services, no shared state
- **Examples**: DbCSVCommunicator
- **Lifetime**: New instance for each service request

---

## Error Handling

### Business Logic Errors

Business logic errors return result tuples:

```csharp
if (driver.Status != DriverStatus.Active)
    return (false, "Driver not active", null);
```

### Exception Handling

Exceptions are caught and handled appropriately:

```csharp
try
{
    // Operation
}
catch (Exception ex)
{
    return (false, $"Operation failed: {ex.Message}", null);
}
```

### Validation Errors

Validation errors are caught early:

```csharp
if (!ValidationHelper.IsValidEmail(email))
    return (false, "Invalid email format", null);
```

---

## Testing Services

### Unit Testing

Services can be unit tested with mocked dependencies:

```csharp
var mockContext = new Mock<AppDbContext>();
var service = new CarpoolService(mockContext.Object);
```

### Integration Testing

Services can be integration tested with in-memory database:

```csharp
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase(databaseName: "TestDb")
    .Options;
var context = new AppDbContext(options);
var service = new CarpoolService(context);
```

---

**Document Version**: 1.0
**Last Updated**: 2025-01-XX
**Maintained By**: Development Team

