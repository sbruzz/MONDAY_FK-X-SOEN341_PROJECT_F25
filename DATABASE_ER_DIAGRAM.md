# Campus Events Database - ER Diagram

## Entity Relationship Diagram

This document provides a comprehensive view of the Campus Events & Ticketing database schema, showing all entities, relationships, and constraints.

## Visual ER Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          CAMPUS EVENTS DATABASE SCHEMA                       │
└─────────────────────────────────────────────────────────────────────────────┘

┌──────────────────────┐
│       USER           │
├──────────────────────┤
│ PK: Id (int)         │
│     Email (string)   │◄─────────────┐
│     PasswordHash     │              │
│     Name (string)    │              │  1:N
│     Role (enum)      │              │
│     ApprovalStatus   │              │
│     CreatedAt        │              │
│     UpdatedAt        │              │
│     LastLoginAt      │──── NEW      │
│     ProfileImageUrl  │──── NEW      │
└──────────────────────┘              │
         │ │                          │
         │ └──────────────┐           │
         │                │           │
         │ 1:N            │ 1:N       │
         │                │           │
         ▼                ▼           │
┌──────────────────┐  ┌─────────────────────┐
│  SAVED_EVENT     │  │   TICKET            │
├──────────────────┤  ├─────────────────────┤
│ PK: UserId       │  │ PK: Id (int)        │
│ PK: EventId      │  │ FK: EventId         │
│     SavedAt      │  │ FK: UserId          │
└──────────────────┘  │     UniqueCode      │
         │            │     QrCodeImage     │
         │            │     ClaimedAt       │
         │            │     RedeemedAt      │
         │            │     IsRedeemed      │
         │            │     PaymentStatus   │──── NEW
         │            │     PaymentAmount   │──── NEW
         └────────┐   └─────────────────────┘
                  │            │
                  │            │
                  │            │ N:1
               N:1│            │
                  │            │
                  ▼            ▼
         ┌──────────────────────────┐
         │        EVENT             │
         ├──────────────────────────┤
         │ PK: Id (int)             │
         │ FK: OrganizerId          │────────────┐
         │ FK: OrganizationId       │────┐       │
         │ FK: CategoryId           │──┐ │       │
         │     Title (string)       │  │ │       │
         │     Description          │  │ │       │
         │     EventDate            │  │ │       │
         │     Location             │  │ │       │
         │     Capacity (int)       │  │ │       │
         │     TicketsIssued        │  │ │       │
         │     TicketType (enum)    │  │ │       │
         │     Price (decimal)      │  │ │       │
         │     IsApproved (bool)    │  │ │       │
         │     CreatedAt            │  │ │       │
         │     UpdatedAt            │  │ │       │
         │     ImageUrl             │──── NEW    │
         └──────────────────────────┘  │ │       │
                  │                    │ │       │
                  │ 1:N                │ │       │
                  │                    │ │       │
                  ▼                    │ │       │
         ┌──────────────────┐          │ │       │
         │ EVENT_ANALYTICS  │──── NEW  │ │       │
         ├──────────────────┤          │ │       │
         │ PK: Id (int)     │          │ │       │
         │ FK: EventId      │          │ │       │
         │     ViewCount    │          │ │       │
         │     SaveCount    │          │ │       │
         │     TicketsSold  │          │ │       │
         │     Revenue      │          │ │       │
         │     AttendanceRate│         │ │       │
         │     LastUpdated  │          │ │       │
         └──────────────────┘          │ │       │
                                       │ │       │
                  ┌────────────────────┘ │       │
                  │ N:1                  │       │
                  ▼                      │       │
         ┌─────────────────────┐         │       │
         │     CATEGORY        │──── NEW │       │
         ├─────────────────────┤         │       │
         │ PK: Id (int)        │         │       │
         │     Name (string)   │         │       │
         │     Description     │         │       │
         │     IconName        │         │       │
         │     IsActive (bool) │         │       │
         └─────────────────────┘         │       │
                                         │       │
                  ┌──────────────────────┘       │
                  │ N:1                          │
                  ▼                              │
         ┌──────────────────────┐                │
         │   ORGANIZATION       │                │
         ├──────────────────────┤                │
         │ PK: Id (int)         │                │
         │     Name (string)    │                │
         │     Description      │                │
         │     CreatedAt        │                │
         │     LogoUrl          │──── NEW        │
         │     IsActive (bool)  │──── NEW        │
         └──────────────────────┘                │
                  │                              │
                  │ N:M                          │
                  │                              │
                  ▼                              │
         ┌─────────────────────────┐             │
         │ ORGANIZATION_MEMBER     │──── NEW     │
         ├─────────────────────────┤             │
         │ PK: OrganizationId      │             │
         │ PK: UserId              │◄────────────┘
         │     Role (enum)         │
         │     JoinedAt            │
         └─────────────────────────┘


         ┌──────────────────────────┐
         │   NOTIFICATION           │──── NEW (Additional Feature)
         ├──────────────────────────┤
         │ PK: Id (int)             │
         │ FK: UserId               │
         │ FK: EventId (optional)   │
         │     Type (enum)          │
         │     Title (string)       │
         │     Message (string)     │
         │     IsRead (bool)        │
         │     CreatedAt            │
         └──────────────────────────┘


         ┌──────────────────────────┐
         │   AUDIT_LOG              │──── NEW (Admin tracking)
         ├──────────────────────────┤
         │ PK: Id (int)             │
         │ FK: UserId               │
         │     Action (string)      │
         │     EntityType (string)  │
         │     EntityId (int)       │
         │     Details (JSON)       │
         │     IpAddress            │
         │     Timestamp            │
         └──────────────────────────┘


         ┌──────────────────────────┐
         │   QR_SCAN_LOG            │──── NEW (Track scans)
         ├──────────────────────────┤
         │ PK: Id (int)             │
         │ FK: TicketId             │
         │ FK: ScannedBy (UserId)   │
         │     ScanMethod (enum)    │
         │     ScanLocation         │
         │     ScannedAt            │
         │     IsValid (bool)       │
         └──────────────────────────┘
```

## Database Schema Details

### Core Entities

#### User
- **Primary Key**: Id (int)
- **Unique**: Email
- **Fields**: Email, PasswordHash, Name, Role, ApprovalStatus, CreatedAt, UpdatedAt, LastLoginAt, ProfileImageUrl
- **Relationships**: 
  - 1:N with Events (as Organizer)
  - 1:N with Tickets
  - M:N with Events (through SavedEvents)
  - M:N with Organizations (through OrganizationMembers)
  - 1:N with Notifications
  - 1:N with AuditLogs
  - 1:N with QrScanLogs (as Scanner)

#### Event
- **Primary Key**: Id (int)
- **Foreign Keys**: OrganizerId, OrganizationId, CategoryId
- **Fields**: Title, Description, EventDate, Location, Capacity, TicketsIssued, TicketType, Price, IsApproved, CreatedAt, UpdatedAt, ImageUrl
- **Relationships**:
  - N:1 with User (Organizer)
  - N:1 with Organization
  - N:1 with Category
  - 1:1 with EventAnalytics
  - 1:N with Tickets
  - M:N with Users (through SavedEvents)
  - 1:N with Notifications

#### Ticket
- **Primary Key**: Id (int)
- **Foreign Keys**: EventId, UserId
- **Unique**: UniqueCode
- **Fields**: UniqueCode, QrCodeImage, ClaimedAt, RedeemedAt, IsRedeemed, PaymentStatus, PaymentAmount
- **Relationships**:
  - N:1 with Event
  - N:1 with User
  - 1:N with QrScanLogs

#### Organization
- **Primary Key**: Id (int)
- **Fields**: Name, Description, CreatedAt, LogoUrl, IsActive
- **Relationships**:
  - 1:N with Events
  - M:N with Users (through OrganizationMembers)

#### Category
- **Primary Key**: Id (int)
- **Fields**: Name, Description, IconName, IsActive
- **Relationships**:
  - 1:N with Events

### New Entities

#### EventAnalytics
- **Primary Key**: Id (int)
- **Foreign Key**: EventId (unique)
- **Fields**: ViewCount, SaveCount, TicketsSold, Revenue, AttendanceRate, LastUpdated
- **Purpose**: Track event performance metrics

#### Notification
- **Primary Key**: Id (int)
- **Foreign Keys**: UserId, EventId (optional)
- **Fields**: Type, Title, Message, IsRead, CreatedAt
- **Purpose**: Event reminders, updates, and system notifications

#### OrganizationMember
- **Composite Primary Key**: OrganizationId, UserId
- **Foreign Keys**: OrganizationId, UserId
- **Fields**: Role, JoinedAt
- **Purpose**: Many-to-many relationship between Users and Organizations with roles

#### AuditLog
- **Primary Key**: Id (int)
- **Foreign Key**: UserId
- **Fields**: Action, EntityType, EntityId, Details (JSON), IpAddress, Timestamp
- **Purpose**: Track admin actions for compliance

#### QrScanLog
- **Primary Key**: Id (int)
- **Foreign Keys**: TicketId, ScannedBy (UserId)
- **Fields**: ScanMethod, ScanLocation, ScannedAt, IsValid
- **Purpose**: Track ticket validation history

## Enums

### Existing Enums
- **UserRole**: Student, Organizer, Admin
- **ApprovalStatus**: Pending, Approved, Rejected
- **TicketType**: Free, Paid

### New Enums
- **PaymentStatus**: Pending, Completed, Failed, Refunded
- **NotificationType**: EventReminder, EventUpdate, TicketClaimed, EventApproved, SystemAlert
- **ScanMethod**: WebCamera, FileUpload, Manual
- **OrganizationRole**: Member, Manager, Admin

## Key Relationships Summary

1. **USER ←→ EVENT** (1:N) - One user (organizer) creates many events
2. **USER ←→ TICKET** (1:N) - One user can have many tickets
3. **USER ←→ SAVED_EVENT ←→ EVENT** (M:N) - Users can save many events
4. **EVENT ←→ TICKET** (1:N) - One event has many tickets
5. **EVENT ←→ ORGANIZATION** (N:1) - Many events belong to one organization
6. **EVENT ←→ CATEGORY** (N:1) - Many events belong to one category
7. **EVENT ←→ EVENT_ANALYTICS** (1:1) - Each event has one analytics record
8. **USER ←→ ORGANIZATION_MEMBER ←→ ORGANIZATION** (M:N) - Users can be members of multiple organizations
9. **USER ←→ NOTIFICATION** (1:N) - One user receives many notifications
10. **TICKET ←→ QR_SCAN_LOG** (1:N) - One ticket can be scanned multiple times (for tracking)

## Indexes for Performance

- **User.Email** - Unique index for login
- **Ticket.UniqueCode** - Unique index for QR validation
- **Event.EventDate** - Index for date-based queries
- **Event.IsApproved** - Index for admin filtering
- **Notification.UserId + IsRead** - Composite index for user notifications
- **AuditLog.UserId + Timestamp** - Composite index for audit queries
- **QrScanLog.TicketId** - Index for scan history

## Constraints

- **Cascade Delete**: When an event is deleted, related tickets and analytics are deleted
- **Restrict Delete**: Users cannot be deleted if they have organized events
- **Set Null**: When an organization is deleted, events' OrganizationId is set to null
- **Unique Constraints**: Email addresses, ticket codes must be unique
- **Check Constraints**: Price must be >= 0, Capacity must be > 0
