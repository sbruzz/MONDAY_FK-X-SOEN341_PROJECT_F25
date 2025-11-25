# Campus Events API Documentation

## Overview

This document provides comprehensive API documentation for the Campus Events & Ticketing System. The application is built using ASP.NET Core 9.0 with Razor Pages architecture, providing a RESTful interface through page models and service layers.

## Table of Contents

1. [Authentication & Authorization](#authentication--authorization)
2. [User Management APIs](#user-management-apis)
3. [Event Management APIs](#event-management-apis)
4. [Ticket Management APIs](#ticket-management-apis)
5. [Carpool System APIs](#carpool-system-apis)
6. [Room Rental System APIs](#room-rental-system-apis)
7. [Notification System APIs](#notification-system-apis)
8. [Admin APIs](#admin-apis)
9. [Error Handling](#error-handling)
10. [Security Considerations](#security-considerations)

---

## Authentication & Authorization

### Session-Based Authentication

The application uses session-based authentication with BCrypt password hashing. Sessions are configured with a 30-minute idle timeout and are essential for maintaining user state across requests.

### User Roles

- **Student**: Can browse events, claim tickets, join carpools, rent rooms
- **Organizer**: Can create events, manage rooms, offer carpools, approve rentals
- **Admin**: Full system access including user/event approval, driver management

### Authorization Checks

All endpoints verify user authentication and role-based permissions before processing requests. Unauthorized access attempts are logged and rejected.

---

## User Management APIs

### User Registration

**Endpoint**: `/SignupStudent` or `/SignupOrganizer`

**Method**: POST

**Request Body**:
```json
{
  "email": "user@example.com",
  "password": "securePassword123",
  "name": "John Doe",
  "studentId": "40294756",  // For students
  "program": "Computer Science",  // For students
  "yearOfStudy": "3rd Year",  // For students
  "phoneNumber": "514-555-1234",  // Optional
  "organizationId": 1,  // For organizers
  "position": "Event Coordinator",  // For organizers
  "department": "Student Services"  // For organizers
}
```

**Response**: Redirects to login page on success

**Validation Rules**:
- Email must be unique and valid format
- Password must meet security requirements (minimum length, complexity)
- Student ID must be 9 digits for Concordia students
- Organizer accounts require admin approval

### User Login

**Endpoint**: `/Login`

**Method**: POST

**Request Body**:
```json
{
  "email": "user@example.com",
  "password": "securePassword123"
}
```

**Response**: 
- Success: Redirects to role-specific home page
- Failure: Returns error message

**Session Data**:
- User ID stored in session
- User role stored in session
- Session timeout: 30 minutes

### User Profile Management

**Endpoint**: `/Student/Home` or `/Organizer/Home`

**Method**: GET/POST

**Functionality**:
- View user profile information
- Update profile details (name, phone, program, etc.)
- View activity history
- Manage saved events

---

## Event Management APIs

### Create Event

**Endpoint**: `/Organizer/CreateEvent`

**Method**: POST

**Authorization**: Organizer role required, account must be approved

**Request Body**:
```json
{
  "title": "Tech Conference 2025",
  "description": "Annual technology conference featuring industry leaders",
  "eventDate": "2025-03-15T10:00:00Z",
  "location": "Hall Building, Room H-110",
  "capacity": 200,
  "ticketType": "Free",  // or "Paid"
  "price": 0.00,  // Required if ticketType is "Paid"
  "category": "Academic",  // Academic, Career, Competition, Concert, Cultural, Social, Sports, Workshop, Other
  "organizationId": 1  // Optional
}
```

**Response**:
- Success: Event created with `ApprovalStatus.Pending`
- Event requires admin approval before being visible to students

**Validation Rules**:
- Title: Required, max 200 characters
- Description: Required, max 5000 characters
- Event Date: Must be in the future
- Capacity: Must be positive integer
- Price: Required if ticketType is "Paid", must be non-negative

### List Events

**Endpoint**: `/Events` or `/Student/Events`

**Method**: GET

**Query Parameters**:
- `category`: Filter by event category
- `dateFrom`: Filter events from this date
- `dateTo`: Filter events until this date
- `organizationId`: Filter by organization
- `search`: Search in title and description

**Response**: List of approved events with pagination

**Filters**:
- Only shows events with `ApprovalStatus.Approved`
- Events must have future or current event dates
- Events must have available capacity

### Event Details

**Endpoint**: `/EventDetails/{id}` or `/Student/EventDetails/{id}`

**Method**: GET

**Response**: Complete event information including:
- Event details
- Organizer information
- Available tickets count
- Related carpool offers
- Related room rentals

### Update Event

**Endpoint**: `/Organizer/EventDetails/{id}`

**Method**: POST

**Authorization**: Must be the event organizer

**Request Body**: Same as Create Event (all fields optional for updates)

**Response**: Updated event information

**Note**: Updates require re-approval from admin if significant changes are made

### Delete Event

**Endpoint**: `/Organizer/EventDetails/{id}`

**Method**: POST (Delete action)

**Authorization**: Must be the event organizer

**Response**: Event marked as deleted (soft delete) or removed

**Cascading Effects**:
- All tickets remain valid
- Carpool offers remain active
- Notifications sent to ticket holders

---

## Ticket Management APIs

### Claim Ticket

**Endpoint**: `/Student/EventDetails/{id}`

**Method**: POST (Claim Ticket action)

**Authorization**: Student role required

**Request**: Event ID from route

**Response**: 
- Success: Ticket created with unique QR code
- Failure: Error message (capacity exceeded, already claimed, etc.)

**Business Logic**:
- Checks event capacity
- Validates user hasn't already claimed a ticket for this event
- Generates unique ticket code
- Creates signed token for QR code
- Updates event ticket count

### Purchase Paid Ticket

**Endpoint**: `/Student/EventDetails/{id}`

**Method**: POST (Purchase Ticket action)

**Authorization**: Student role required

**Request Body**:
```json
{
  "eventId": 1,
  "paymentMethod": "mock"  // Mock payment for development
}
```

**Response**: 
- Success: Ticket created, payment processed (mock)
- Failure: Payment failed or capacity exceeded

**Note**: In production, integrate with real payment gateway

### View Tickets

**Endpoint**: `/Student/Tickets`

**Method**: GET

**Authorization**: Student role required

**Response**: List of user's tickets with:
- Event information
- Ticket status (claimed/redeemed)
- QR code for scanning
- Redemption status

### Generate QR Code

**Endpoint**: `/Student/Tickets/{id}/QR`

**Method**: GET

**Authorization**: Must own the ticket

**Response**: QR code image (PNG) containing signed token

**Token Contents**:
- Event ID
- Ticket ID
- Unique code
- Expiry timestamp (24 hours after event)
- HMAC-SHA256 signature

### Validate Ticket

**Endpoint**: `/Organizer/QRScanner`

**Method**: POST

**Authorization**: Organizer role required

**Request Body**:
```json
{
  "qrCodeData": "base64_encoded_qr_code_image"  // or signed token string
}
```

**Response**:
```json
{
  "isValid": true,
  "ticketId": 123,
  "eventId": 1,
  "uniqueCode": "TKT-2025-001",
  "userId": 45,
  "userName": "John Doe",
  "isRedeemed": false,
  "errorMessage": null
}
```

**Validation Process**:
1. Decode QR code or parse token
2. Verify HMAC signature
3. Check token expiry
4. Validate ticket exists in database
5. Check if already redeemed
6. Mark as redeemed if valid

---

## Carpool System APIs

### Register as Driver

**Endpoint**: `/Student/BecomeDriver` or `/Organizer/Drivers`

**Method**: POST

**Authorization**: Student or Organizer role required

**Request Body**:
```json
{
  "capacity": 4,
  "vehicleType": "Sedan",  // Mini, Sedan, SUV, Van, Bus
  "driverType": "Student",  // Student or Organizer
  "licensePlate": "ABC-1234",  // Optional
  "driverLicenseNumber": "D12345678",  // Required, encrypted
  "province": "QC",  // Required for license validation
  "accessibilityFeatures": "wheelchair_accessible,service_animal_friendly"  // Optional
}
```

**Response**:
- Success: Driver profile created with `DriverStatus.Pending`
- Requires admin approval before creating offers

**Validation Rules**:
- Capacity: 1-50 passengers
- Driver license: Valid format for province
- Province: Valid Canadian province/territory code
- License plate: Optional, encrypted if provided

### Create Carpool Offer

**Endpoint**: `/Student/OfferRide` or `/Organizer/Drivers`

**Method**: POST

**Authorization**: Must be an approved driver

**Request Body**:
```json
{
  "eventId": 1,
  "departureInfo": "Leaving from Hall Building at 9:00 AM",
  "departureTime": "2025-03-15T09:00:00Z",
  "departureAddress": "1455 De Maisonneuve Blvd W, Montreal, QC",
  "latitude": 45.4972,
  "longitude": -73.5794
}
```

**Response**:
- Success: Carpool offer created with available seats
- Failure: Driver not active, offer already exists, etc.

**Business Logic**:
- Validates driver is active
- Checks event exists
- Prevents duplicate offers for same event
- Initializes seats available from driver capacity

### List Carpool Offers

**Endpoint**: `/Student/Carpools` or `/Student/CarpoolOffers`

**Method**: GET

**Query Parameters**:
- `eventId`: Filter by event
- `status`: Filter by offer status (Active, Full, Cancelled)

**Response**: List of active carpool offers with:
- Driver information
- Departure details
- Available seats
- Event information
- Passenger count

### Join Carpool

**Endpoint**: `/Student/CarpoolOffers/{id}/Join`

**Method**: POST

**Authorization**: Student role required

**Request Body**:
```json
{
  "pickupLocation": "Near Metro Station",
  "notes": "I'll be waiting at the main entrance"
}
```

**Response**:
- Success: Passenger added, seats decremented
- Failure: No seats available, already joined, etc.

**Business Logic**:
- Validates offer is active
- Checks available seats
- Prevents driver from joining own offer
- Prevents duplicate joins
- Updates offer status to "Full" if no seats remain

### Leave Carpool

**Endpoint**: `/Student/Carpools/{id}/Leave`

**Method**: POST

**Authorization**: Must be the passenger

**Response**:
- Success: Passenger removed, seats incremented
- Offer status updated to "Active" if it was full

### Cancel Carpool Offer

**Endpoint**: `/Student/Carpools/{id}/Cancel` or `/Organizer/Drivers/{id}/Cancel`

**Method**: POST

**Authorization**: Must be the driver

**Response**:
- Success: Offer cancelled
- Failure: Cannot cancel if confirmed passengers exist

**Note**: Driver should contact passengers before cancelling

### Get User Carpools

**Endpoint**: `/Student/Carpools`

**Method**: GET

**Response**: 
```json
{
  "asDriver": [
    {
      "offerId": 1,
      "event": {...},
      "passengers": [...],
      "seatsAvailable": 2
    }
  ],
  "asPassenger": [
    {
      "offerId": 2,
      "event": {...},
      "driver": {...},
      "status": "Confirmed"
    }
  ]
}
```

---

## Room Rental System APIs

### Create Room

**Endpoint**: `/Organizer/Rooms`

**Method**: POST

**Authorization**: Organizer role required

**Request Body**:
```json
{
  "name": "Conference Room A",
  "address": "Hall Building, Room H-110",
  "capacity": 50,
  "roomInfo": "Large conference room with projector and whiteboard",
  "amenities": "projector,whiteboard,wifi,ac",
  "hourlyRate": 25.00,  // Optional
  "availabilityStart": "2025-01-01T00:00:00Z",  // Optional
  "availabilityEnd": "2025-12-31T23:59:59Z"  // Optional
}
```

**Response**:
- Success: Room created with `RoomStatus.Enabled`
- Room immediately available for rental requests

**Validation Rules**:
- Name: Required, max 200 characters
- Address: Required
- Capacity: Must be positive integer
- Availability window: End must be after start if both provided

### List Rooms

**Endpoint**: `/Student/Rooms` or `/Organizer/Rooms`

**Method**: GET

**Query Parameters**:
- `minCapacity`: Filter by minimum capacity
- `availableFrom`: Filter by availability start
- `availableTo`: Filter by availability end
- `onlyEnabled`: Show only enabled rooms (default: true)

**Response**: List of available rooms with:
- Room details
- Organizer information
- Current availability
- Pricing information

### Request Room Rental

**Endpoint**: `/Student/Rooms/{id}/Rent`

**Method**: POST

**Authorization**: Student or Organizer role required

**Request Body**:
```json
{
  "startTime": "2025-03-15T10:00:00Z",
  "endTime": "2025-03-15T12:00:00Z",
  "purpose": "Study group meeting",
  "expectedAttendees": 15
}
```

**Response**:
- Success: Rental request created with `RentalStatus.Pending`
- Requires organizer or admin approval

**Validation Rules**:
- Start time must be in the future
- End time must be after start time
- Must not overlap with existing approved/pending rentals
- Expected attendees must not exceed room capacity
- Must be within room availability window if specified

**Business Logic**:
- Checks for double booking (overlapping rentals)
- Calculates total cost if hourly rate is set
- Validates room is enabled
- Validates capacity constraints

### Approve Rental Request

**Endpoint**: `/Organizer/Rooms/{id}/Rentals/{rentalId}/Approve`

**Method**: POST

**Authorization**: Must be the room organizer or admin

**Response**:
- Success: Rental approved, notification sent to renter
- Failure: Conflicting rental already approved, room disabled, etc.

**Business Logic**:
- Re-checks for overlaps (race condition protection)
- Validates room is still enabled
- Sends approval notification
- Updates rental status

### Reject Rental Request

**Endpoint**: `/Organizer/Rooms/{id}/Rentals/{rentalId}/Reject`

**Method**: POST

**Authorization**: Must be the room organizer or admin

**Request Body**:
```json
{
  "adminNotes": "Room is already booked for maintenance during this time"
}
```

**Response**:
- Success: Rental rejected, notification sent to renter
- Includes rejection reason if provided

### Cancel Rental

**Endpoint**: `/Student/Rentals/{id}/Cancel`

**Method**: POST

**Authorization**: Must be the renter

**Response**:
- Success: Rental cancelled
- Failure: Cannot cancel completed or already cancelled rental

### Get Available Rooms

**Endpoint**: `/Student/Rooms/Available`

**Method**: GET

**Query Parameters**:
- `startTime`: Required
- `endTime`: Required
- `minCapacity`: Optional

**Response**: List of rooms available for the specified time slot

**Business Logic**:
- Filters out rooms with overlapping approved/pending rentals
- Checks room availability windows
- Validates capacity requirements

---

## Notification System APIs

### Get Notifications

**Endpoint**: `/Shared/GetNotifications`

**Method**: GET

**Authorization**: Authenticated user

**Query Parameters**:
- `limit`: Maximum number of notifications (default: 50)
- `unreadOnly`: Return only unread notifications (default: false)

**Response**:
```json
{
  "notifications": [
    {
      "id": 1,
      "type": "RentalApproved",
      "title": "Rental Approved",
      "message": "Your rental request for 'Conference Room A' has been approved!",
      "isRead": false,
      "createdAt": "2025-03-10T10:00:00Z",
      "actionUrl": "/Student/Rentals"
    }
  ],
  "unreadCount": 5
}
```

### Mark Notification as Read

**Endpoint**: `/Shared/GetNotifications/{id}/Read`

**Method**: POST

**Authorization**: Must own the notification

**Response**:
- Success: Notification marked as read
- Failure: Notification not found or unauthorized

### Mark All as Read

**Endpoint**: `/Shared/GetNotifications/ReadAll`

**Method**: POST

**Authorization**: Authenticated user

**Response**: All user's unread notifications marked as read

### Delete Notification

**Endpoint**: `/Shared/GetNotifications/{id}/Delete`

**Method**: POST

**Authorization**: Must own the notification

**Response**: Notification deleted

### Notification Types

- `RentalApproved`: Room rental request approved
- `RentalRejected`: Room rental request rejected
- `RoomDisabled`: Room has been disabled by admin
- `EventApproved`: Event has been approved by admin
- `EventRejected`: Event has been rejected by admin
- `DriverApproved`: Driver registration approved
- `DriverSuspended`: Driver account suspended

---

## Admin APIs

### Approve User

**Endpoint**: `/Admin/Users/{id}/Approve`

**Method**: POST

**Authorization**: Admin role required

**Response**: User account approved, can now access organizer features

### Reject User

**Endpoint**: `/Admin/Users/{id}/Reject`

**Method**: POST

**Authorization**: Admin role required

**Request Body**:
```json
{
  "reason": "Invalid organization information"
}
```

**Response**: User account rejected

### Approve Event

**Endpoint**: `/Admin/Events/{id}/Approve`

**Method**: POST

**Authorization**: Admin role required

**Response**: Event approved, now visible to students

### Reject Event

**Endpoint**: `/Admin/Events/{id}/Reject`

**Method**: POST

**Authorization**: Admin role required

**Request Body**:
```json
{
  "reason": "Event content violates community guidelines"
}
```

**Response**: Event rejected, not visible to students

### Approve Driver

**Endpoint**: `/Admin/Drivers/{id}/Approve`

**Method**: POST

**Authorization**: Admin role required

**Response**: Driver approved, can now create carpool offers

### Suspend Driver

**Endpoint**: `/Admin/Drivers/{id}/Suspend`

**Method**: POST

**Authorization**: Admin role required

**Request Body**:
```json
{
  "reason": "Multiple safety complaints"
}
```

**Response**: Driver suspended, all active offers cancelled

**Cascading Effects**:
- All active carpool offers cancelled
- Security flag added to driver profile
- Notifications sent to affected passengers

### Enable/Disable Room

**Endpoint**: `/Admin/Rooms/{id}/Enable` or `/Admin/Rooms/{id}/Disable`

**Method**: POST

**Authorization**: Admin role required

**Request Body** (for disable):
```json
{
  "reason": "Room under maintenance"
}
```

**Response**: Room status updated

**Cascading Effects** (for disable):
- All pending rentals rejected
- Notifications sent to users with approved future rentals

### View System Analytics

**Endpoint**: `/Admin/Home`

**Method**: GET

**Authorization**: Admin role required

**Response**: Dashboard with:
- Total events count
- Total tickets issued
- Total users by role
- Active carpool offers
- Room rental statistics
- Recent activity

---

## Error Handling

### Standard Error Response Format

```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Event date must be in the future",
    "details": {
      "field": "eventDate",
      "value": "2024-01-01"
    }
  }
}
```

### Common Error Codes

- `VALIDATION_ERROR`: Input validation failed
- `UNAUTHORIZED`: User not authenticated
- `FORBIDDEN`: User lacks required permissions
- `NOT_FOUND`: Resource not found
- `CONFLICT`: Resource conflict (e.g., duplicate, overlap)
- `CAPACITY_EXCEEDED`: Event or carpool at capacity
- `INVALID_STATE`: Operation not allowed in current state
- `SERVER_ERROR`: Internal server error

### Error Handling Best Practices

1. Always validate input before processing
2. Return descriptive error messages
3. Log errors for debugging
4. Never expose sensitive information in error messages
5. Use appropriate HTTP status codes

---

## Security Considerations

### Password Security

- Passwords are hashed using BCrypt with appropriate work factor
- Passwords are never stored in plain text
- Password validation enforces complexity requirements

### Data Encryption

- Driver license numbers are encrypted at rest
- License plates are encrypted at rest
- Ticket signing keys are stored securely (environment variables in production)

### Session Security

- Sessions use HttpOnly cookies
- Sessions expire after 30 minutes of inactivity
- Session data is stored server-side

### QR Code Security

- QR codes contain HMAC-SHA256 signed tokens
- Tokens include expiry timestamps
- Tokens cannot be forged without signing key
- Token verification uses constant-time comparison to prevent timing attacks

### SQL Injection Prevention

- All database queries use parameterized queries via Entity Framework Core
- No raw SQL with user input

### XSS Prevention

- All user input is HTML-encoded in Razor views
- Content Security Policy headers recommended for production

### CSRF Protection

- Anti-forgery tokens used on all POST requests
- ValidateReferer header recommended for production

---

## Rate Limiting

Currently, the application does not implement rate limiting. For production deployment, consider:

- Rate limiting on authentication endpoints
- Rate limiting on ticket claiming
- Rate limiting on carpool/rental requests
- IP-based throttling for suspicious activity

---

## API Versioning

Current version: **v1**

Future versions will be specified in the URL path: `/api/v2/...`

---

## Testing

### Test Endpoints

Development endpoints for testing (disabled in production):

- `/Test/GenerateTestData`: Generate sample events, users, tickets
- `/Test/ClearDatabase`: Clear all test data (development only)

### Integration Testing

See `CampusEvents.Tests/IntegrationTests/` for integration test examples.

---

## Support

For API issues or questions:
- Check the main README.md for setup instructions
- Review Architecture documentation
- Contact the development team

---

**Last Updated**: 2025-01-XX
**API Version**: 1.0
**Documentation Maintainer**: Development Team

