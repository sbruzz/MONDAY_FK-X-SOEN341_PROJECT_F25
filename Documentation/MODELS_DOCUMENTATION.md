# Campus Events System - Models Documentation

## Overview

This document provides comprehensive documentation for all domain models in the Campus Events system. Models represent the core business entities and their relationships.

## Table of Contents

1. [Core Models](#core-models)
2. [Carpool System Models](#carpool-system-models)
3. [Room Rental System Models](#room-rental-system-models)
4. [Supporting Models](#supporting-models)
5. [Enumerations](#enumerations)
6. [Model Relationships](#model-relationships)

---

## Core Models

### User Model

**Location**: `Models/User.cs`

**Purpose**: Represents a user account in the system with role-based access control.

**Properties**:

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Id` | `int` | Yes | Primary key, auto-increment |
| `Email` | `string` | Yes | Unique email address (indexed) |
| `PasswordHash` | `string` | Yes | BCrypt hashed password |
| `Name` | `string` | Yes | User's full name |
| `Role` | `UserRole` | Yes | User role (Student, Organizer, Admin) |
| `ApprovalStatus` | `ApprovalStatus` | Yes | Approval status (for organizers) |
| `CreatedAt` | `DateTime` | Yes | Account creation timestamp |
| `history` | `string` | No | User activity history (JSON) |

**Student-Specific Properties**:

| Property | Type | Description |
|----------|------|-------------|
| `StudentId` | `string?` | 9-digit Concordia student ID |
| `Program` | `string?` | Academic program of study |
| `YearOfStudy` | `string?` | Year of study (e.g., "3rd Year") |
| `PhoneNumber` | `string?` | Contact phone number |

**Organizer-Specific Properties**:

| Property | Type | Description |
|----------|------|-------------|
| `OrganizationId` | `int?` | Foreign key to Organization |
| `Position` | `string?` | Position within organization |
| `Department` | `string?` | Department within organization |

**Navigation Properties**:

- `Organization`: Associated organization (for organizers)
- `Drivers`: Driver profiles (can have multiple for organizers)
- `Tickets`: Tickets owned by user
- `OrganizedEvents`: Events created by user (organizers)
- `SavedEvents`: Events saved to user's calendar
- `RoomRentals`: Room rentals requested by user
- `CarpoolPassengers`: Carpool rides joined as passenger
- `ManagedRooms`: Rooms managed by user (organizers)
- `Notifications`: Notifications received by user

**Business Rules**:

- Email must be unique across all users
- Password must be hashed using BCrypt
- Organizer accounts require admin approval before activation
- Students can have at most one driver profile
- Organizers can have multiple driver profiles

**Validation**:

- Email format validation
- Student ID must be 9 digits (if provided)
- Password complexity requirements enforced at registration

---

### Event Model

**Location**: `Models/Event.cs`

**Purpose**: Represents a campus event that can be attended by students.

**Properties**:

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Id` | `int` | Yes | Primary key, auto-increment |
| `Title` | `string` | Yes | Event title/name |
| `Description` | `string` | Yes | Detailed event description |
| `EventDate` | `DateTime` | Yes | Date and time of event |
| `Location` | `string` | Yes | Physical location/venue |
| `Capacity` | `int` | Yes | Maximum number of attendees |
| `TicketsIssued` | `int` | Yes | Number of tickets issued (default: 0) |
| `TicketType` | `TicketType` | Yes | Free or Paid ticket type |
| `Price` | `decimal` | Yes | Price per ticket (if paid) |
| `Category` | `EventCategory` | Yes | Event category |
| `OrganizerId` | `int` | Yes | Foreign key to User (organizer) |
| `OrganizationId` | `int?` | No | Foreign key to Organization |
| `ApprovalStatus` | `ApprovalStatus` | Yes | Admin approval status |
| `CreatedAt` | `DateTime` | Yes | Event creation timestamp |

**Navigation Properties**:

- `Organizer`: User who created the event
- `Organization`: Associated organization (optional)
- `Tickets`: Tickets issued for this event
- `SavedByUsers`: Users who saved this event
- `CarpoolOffers`: Carpool offers for this event

**Business Rules**:

- Event date must be in the future when created
- Capacity must be positive
- TicketsIssued cannot exceed Capacity
- Price must be non-negative
- Price must be set if TicketType is Paid
- Events require admin approval before being visible to students
- Only approved events appear in student event listings

**Validation**:

- Title: Required, max 200 characters
- Description: Required, max 5000 characters
- EventDate: Must be in the future
- Capacity: Must be positive integer
- Price: Must be non-negative, required if TicketType is Paid

**Event Categories**:

- `Academic`: Academic lectures, seminars
- `Career`: Career fairs, networking events
- `Competition`: Competitions, hackathons
- `Concert`: Musical performances
- `Cultural`: Cultural celebrations
- `Social`: Social gatherings, parties
- `Sports`: Sporting events
- `Workshop`: Educational workshops
- `Other`: Other event types

---

### Ticket Model

**Location**: `Models/Ticket.cs`

**Purpose**: Represents a ticket for an event, owned by a user.

**Properties**:

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Id` | `int` | Yes | Primary key, auto-increment |
| `EventId` | `int` | Yes | Foreign key to Event |
| `UserId` | `int` | Yes | Foreign key to User (owner) |
| `UniqueCode` | `string` | Yes | Unique ticket identifier (indexed) |
| `ClaimedAt` | `DateTime` | Yes | Ticket claim timestamp |
| `RedeemedAt` | `DateTime?` | No | Ticket redemption timestamp |
| `IsRedeemed` | `bool` | Yes | Redemption status flag |

**Navigation Properties**:

- `Event`: Event this ticket is for
- `User`: User who owns this ticket

**Business Rules**:

- UniqueCode must be unique across all tickets
- One ticket per user per event (enforced in business logic)
- Cannot claim ticket if event is at capacity
- Cannot claim ticket if event is in the past
- QR code generated from UniqueCode using TicketSigningService
- Ticket can only be redeemed once
- RedeemedAt is set when ticket is validated

**QR Code Generation**:

- QR code contains HMAC-SHA256 signed token
- Token includes: EventId, TicketId, UniqueCode, Expiry
- Token expires 24 hours after event date
- Token signature prevents forgery

---

### Organization Model

**Location**: `Models/Organization.cs`

**Purpose**: Represents an organization that can host events.

**Properties**:

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Id` | `int` | Yes | Primary key, auto-increment |
| `Name` | `string` | Yes | Organization name |
| `Description` | `string?` | No | Organization description |
| `CreatedAt` | `DateTime` | Yes | Creation timestamp |

**Navigation Properties**:

- `Events`: Events associated with this organization

**Business Rules**:

- Organizations are created by administrators
- Multiple organizers can belong to the same organization
- Events can optionally be associated with an organization

---

## Carpool System Models

### Driver Model

**Location**: `Models/Driver.cs`

**Purpose**: Represents a driver profile for the carpool system.

**Properties**:

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Id` | `int` | Yes | Primary key, auto-increment |
| `UserId` | `int` | Yes | Foreign key to User |
| `DriverLicenseNumber` | `string?` | No | Encrypted license number |
| `Province` | `string?` | No | Province/territory code (2 letters) |
| `LicensePlate` | `string?` | No | Encrypted license plate |
| `Capacity` | `int` | Yes | Passenger capacity (1-50) |
| `VehicleType` | `VehicleType` | Yes | Type of vehicle |
| `DriverType` | `DriverType` | Yes | Student or Organizer driver |
| `Status` | `DriverStatus` | Yes | Admin-managed status |
| `SecurityFlags` | `string` | Yes | Comma-separated security flags |
| `AccessibilityFeatures` | `string` | Yes | Comma-separated accessibility features |
| `History` | `string` | Yes | Ride history (JSON or comma-separated) |
| `CreatedAt` | `DateTime` | Yes | Profile creation timestamp |

**Navigation Properties**:

- `User`: Associated user account
- `CarpoolOffers`: Carpool offers created by this driver

**Business Rules**:

- Capacity must be between 1 and 50
- Driver license number is encrypted at rest
- License plate is encrypted at rest
- Requires admin approval before creating offers
- Students can have at most one driver profile
- Organizers can have multiple driver profiles
- Status changes are logged in SecurityFlags

**Encryption**:

- DriverLicenseNumber: Encrypted using AES-256 via EncryptionService
- LicensePlate: Encrypted using AES-256 via EncryptionService
- Encryption keys stored in configuration (environment variables in production)

**Vehicle Types**:

- `Mini`: Small car (1-2 passengers)
- `Sedan`: Standard car (3-4 passengers)
- `SUV`: Sport utility vehicle (5-7 passengers)
- `Van`: Large van (8-12 passengers)
- `Bus`: Bus (13+ passengers)

**Driver Status**:

- `Pending`: Awaiting admin approval
- `Active`: Approved and can create offers
- `Suspended`: Temporarily disabled by admin

**Accessibility Features** (comma-separated):

- `wheelchair_accessible`: Vehicle can accommodate wheelchair
- `service_animal_friendly`: Service animals welcome
- `hearing_assistance`: Hearing assistance available

---

### CarpoolOffer Model

**Location**: `Models/CarpoolOffer.cs`

**Purpose**: Represents a carpool ride offer for an event.

**Properties**:

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Id` | `int` | Yes | Primary key, auto-increment |
| `EventId` | `int` | Yes | Foreign key to Event |
| `DriverId` | `int` | Yes | Foreign key to Driver |
| `SeatsAvailable` | `int` | Yes | Current available seats |
| `DepartureInfo` | `string` | Yes | Departure details (time, location) |
| `DepartureAddress` | `string?` | No | Physical departure address |
| `Latitude` | `double?` | No | Departure latitude |
| `Longitude` | `double?` | No | Departure longitude |
| `DepartureTime` | `DateTime` | Yes | Estimated departure time |
| `Status` | `CarpoolOfferStatus` | Yes | Offer status |
| `CreatedAt` | `DateTime` | Yes | Offer creation timestamp |

**Navigation Properties**:

- `Event`: Associated event
- `Driver`: Driver who created the offer
- `Passengers`: Passengers who joined the ride

**Business Rules**:

- SeatsAvailable decrements when passengers join
- SeatsAvailable cannot exceed driver's capacity
- Status changes to "Full" when SeatsAvailable reaches 0
- Only one active offer per driver per event
- Driver must be active to create offers
- Offer can be cancelled if no confirmed passengers

**Offer Status**:

- `Active`: Accepting passengers
- `Full`: All seats taken
- `Cancelled`: Offer cancelled by driver
- `Completed`: Ride completed

**Geolocation**:

- Latitude/Longitude optional for proximity calculations
- Used by ProximityService to find nearby offers
- Coordinates validated using ValidationHelper

---

### CarpoolPassenger Model

**Location**: `Models/CarpoolPassenger.cs`

**Purpose**: Represents a passenger joining a carpool offer.

**Properties**:

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Id` | `int` | Yes | Primary key, auto-increment |
| `OfferId` | `int` | Yes | Foreign key to CarpoolOffer |
| `PassengerId` | `int` | Yes | Foreign key to User |
| `Status` | `PassengerStatus` | Yes | Passenger status |
| `PickupLocation` | `string?` | No | Passenger pickup location |
| `Notes` | `string?` | No | Additional notes |
| `JoinedAt` | `DateTime` | Yes | Join timestamp |

**Navigation Properties**:

- `Offer`: Carpool offer joined
- `Passenger`: User who joined as passenger

**Business Rules**:

- One passenger record per user per offer
- Passenger cannot be the driver
- Status changes to "Cancelled" when passenger leaves
- SeatsAvailable on offer decrements when passenger joins
- SeatsAvailable on offer increments when passenger leaves

**Passenger Status**:

- `Confirmed`: Passenger confirmed for ride
- `Cancelled`: Passenger cancelled participation

---

## Room Rental System Models

### Room Model

**Location**: `Models/Room.cs`

**Purpose**: Represents a rentable room managed by an organizer.

**Properties**:

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Id` | `int` | Yes | Primary key, auto-increment |
| `OrganizerId` | `int` | Yes | Foreign key to User (organizer) |
| `Name` | `string` | Yes | Room name/identifier |
| `Address` | `string` | Yes | Physical address |
| `RoomInfo` | `string?` | No | Detailed room information |
| `Capacity` | `int` | Yes | Maximum capacity |
| `Status` | `RoomStatus` | Yes | Room status |
| `AvailabilityStart` | `DateTime?` | No | Availability window start |
| `AvailabilityEnd` | `DateTime?` | No | Availability window end |
| `Amenities` | `string` | Yes | Comma-separated amenities |
| `HourlyRate` | `decimal?` | No | Hourly rental rate |
| `CreatedAt` | `DateTime` | Yes | Room creation timestamp |

**Navigation Properties**:

- `Organizer`: User who manages this room
- `Rentals`: Rental bookings for this room

**Business Rules**:

- Capacity must be positive
- AvailabilityEnd must be after AvailabilityStart if both provided
- Status can be changed by admin (overrides all rentals)
- Only organizers can create rooms
- HourlyRate is optional (free rooms)

**Room Status**:

- `Enabled`: Room available for rental
- `Disabled`: Room disabled by admin (overrides rentals)
- `UnderMaintenance`: Room temporarily unavailable

**Amenities** (comma-separated):

- `projector`: Projector available
- `whiteboard`: Whiteboard available
- `wifi`: WiFi internet access
- `ac`: Air conditioning
- `parking`: Parking available
- `catering`: Catering services available

---

### RoomRental Model

**Location**: `Models/RoomRental.cs`

**Purpose**: Represents a room rental booking.

**Properties**:

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Id` | `int` | Yes | Primary key, auto-increment |
| `RoomId` | `int` | Yes | Foreign key to Room |
| `RenterId` | `int` | Yes | Foreign key to User (renter) |
| `StartTime` | `DateTime` | Yes | Rental start time |
| `EndTime` | `DateTime` | Yes | Rental end time |
| `Status` | `RentalStatus` | Yes | Rental status |
| `Purpose` | `string?` | No | Rental purpose |
| `ExpectedAttendees` | `int?` | No | Expected number of attendees |
| `TotalCost` | `decimal?` | No | Calculated total cost |
| `CreatedAt` | `DateTime` | Yes | Request creation timestamp |
| `AdminNotes` | `string?` | No | Admin/organizer notes |

**Navigation Properties**:

- `Room`: Room being rented
- `Renter`: User renting the room

**Business Rules**:

- EndTime must be after StartTime
- StartTime must be in the future
- Cannot overlap with existing Approved/Pending rentals
- ExpectedAttendees cannot exceed room capacity
- Must be within room availability window (if specified)
- TotalCost calculated from HourlyRate and duration
- Status changes require organizer/admin approval

**Rental Status**:

- `Pending`: Awaiting organizer/admin approval
- `Approved`: Rental approved
- `Rejected`: Rental rejected
- `Completed`: Rental completed
- `Cancelled`: Rental cancelled

**Overlap Detection**:

- Composite index on (RoomId, StartTime, EndTime)
- Overlap check prevents double booking
- Overlap occurs if:
  - StartTime is within existing rental period
  - EndTime is within existing rental period
  - Rental period completely contains existing rental

---

## Supporting Models

### SavedEvent Model

**Location**: `Models/SavedEvent.cs`

**Purpose**: Represents a many-to-many relationship between users and saved events.

**Properties**:

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `UserId` | `int` | Yes | Foreign key to User (part of composite key) |
| `EventId` | `int` | Yes | Foreign key to Event (part of composite key) |
| `SavedAt` | `DateTime` | Yes | Save timestamp |

**Navigation Properties**:

- `User`: User who saved the event
- `Event`: Event that was saved

**Business Rules**:

- Composite primary key (UserId, EventId)
- One save record per user per event
- Used for user's personal event calendar

---

### Notification Model

**Location**: `Models/Notification.cs`

**Purpose**: Represents a notification sent to a user.

**Properties**:

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Id` | `int` | Yes | Primary key, auto-increment |
| `UserId` | `int` | Yes | Foreign key to User |
| `Type` | `NotificationType` | Yes | Notification type |
| `Title` | `string` | Yes | Notification title (max 200 chars) |
| `Message` | `string` | Yes | Notification message (max 1000 chars) |
| `IsRead` | `bool` | Yes | Read status flag |
| `CreatedAt` | `DateTime` | Yes | Creation timestamp |
| `RelatedEntityId` | `int?` | No | Related entity ID |
| `RelatedEntityType` | `string?` | No | Related entity type (max 50 chars) |
| `ActionUrl` | `string?` | No | URL for notification action (max 500 chars) |

**Navigation Properties**:

- `User`: User who received the notification

**Business Rules**:

- Indexed on (UserId, IsRead, CreatedAt) for efficient queries
- Notifications are created by services (NotificationService)
- Users can mark notifications as read
- Old read notifications can be deleted

**Notification Types**:

- `Info`: General information
- `Success`: Success message
- `Warning`: Warning message
- `Error`: Error message
- `RoomDisabled`: Room has been disabled
- `RentalApproved`: Room rental approved
- `RentalRejected`: Room rental rejected
- `EventCancelled`: Event has been cancelled
- `EventUpdated`: Event has been updated

---

## Enumerations

### UserRole

**Values**:
- `Student`: Student user role
- `Organizer`: Organizer user role
- `Admin`: Administrator user role

### ApprovalStatus

**Values**:
- `Pending`: Awaiting approval
- `Approved`: Approved
- `Rejected`: Rejected

### EventCategory

**Values**:
- `Academic`: Academic events
- `Career`: Career-related events
- `Competition`: Competitions
- `Concert`: Concerts
- `Cultural`: Cultural events
- `Social`: Social events
- `Sports`: Sports events
- `Workshop`: Workshops
- `Other`: Other events

### TicketType

**Values**:
- `Free`: Free ticket
- `Paid`: Paid ticket

### VehicleType

**Values**:
- `Mini`: Small car
- `Sedan`: Standard car
- `SUV`: Sport utility vehicle
- `Van`: Large van
- `Bus`: Bus

### DriverStatus

**Values**:
- `Active`: Active driver
- `Suspended`: Suspended driver
- `Pending`: Pending approval

### DriverType

**Values**:
- `Student`: Student driver
- `Organizer`: Organizer driver

### CarpoolOfferStatus

**Values**:
- `Active`: Active offer
- `Full`: Full offer
- `Cancelled`: Cancelled offer
- `Completed`: Completed offer

### PassengerStatus

**Values**:
- `Confirmed`: Confirmed passenger
- `Cancelled`: Cancelled passenger

### RoomStatus

**Values**:
- `Enabled`: Room enabled
- `Disabled`: Room disabled
- `UnderMaintenance`: Room under maintenance

### RentalStatus

**Values**:
- `Pending`: Pending approval
- `Approved`: Approved rental
- `Rejected`: Rejected rental
- `Completed`: Completed rental
- `Cancelled`: Cancelled rental

### NotificationType

**Values**:
- `Info`: Information notification
- `Success`: Success notification
- `Warning`: Warning notification
- `Error`: Error notification
- `RoomDisabled`: Room disabled notification
- `RentalApproved`: Rental approved notification
- `RentalRejected`: Rental rejected notification
- `EventCancelled`: Event cancelled notification
- `EventUpdated`: Event updated notification

---

## Model Relationships

### One-to-Many Relationships

1. **User → Events**: One user (organizer) can create many events
2. **User → Tickets**: One user can own many tickets
3. **User → Drivers**: One user can have multiple driver profiles (organizers) or one (students)
4. **Event → Tickets**: One event can have many tickets
5. **Event → CarpoolOffers**: One event can have many carpool offers
6. **Driver → CarpoolOffers**: One driver can create many carpool offers
7. **CarpoolOffer → CarpoolPassengers**: One offer can have many passengers
8. **Room → RoomRentals**: One room can have many rentals
9. **User → RoomRentals**: One user can request many rentals
10. **User → Notifications**: One user can receive many notifications
11. **Organization → Events**: One organization can host many events

### Many-to-Many Relationships

1. **User ↔ Event (SavedEvents)**: Users can save multiple events, events can be saved by multiple users
2. **CarpoolOffer ↔ User (CarpoolPassengers)**: Offers can have multiple passengers, users can join multiple offers

### Optional Relationships

1. **Event → Organization**: Events can optionally be associated with an organization
2. **User → Organization**: Organizers can optionally belong to an organization

---

## Data Validation

### Model-Level Validation

Models use data annotations and custom validation:

- `[Required]`: Required fields
- `[StringLength]`: String length constraints
- `[ForeignKey]`: Foreign key relationships
- Custom validation in services

### Business Rule Validation

Business rules are enforced in service layer:

- Capacity checks
- Overlap detection
- Status transitions
- Authorization checks

---

## Database Constraints

### Primary Keys

- All entities have `Id` as auto-increment primary key
- `SavedEvent` uses composite primary key (UserId, EventId)

### Unique Constraints

- `User.Email`: Unique email constraint
- `Ticket.UniqueCode`: Unique ticket code

### Foreign Key Constraints

- All foreign keys have referential integrity
- Delete behaviors configured (Cascade, Restrict, SetNull)

### Indexes

- `User.Email`: Unique index
- `Ticket.UniqueCode`: Unique index
- `RoomRental(RoomId, StartTime, EndTime)`: Composite index for overlap queries
- `Notification(UserId, IsRead, CreatedAt)`: Composite index for notification queries

---

**Document Version**: 1.0
**Last Updated**: 2025-01-XX
**Maintained By**: Development Team

