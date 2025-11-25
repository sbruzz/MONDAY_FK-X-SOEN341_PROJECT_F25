# Campus Events System - API Reference

## Overview

This document provides a comprehensive reference for all API endpoints, services, and helper utilities in the Campus Events system.

## Table of Contents

1. [Service APIs](#service-apis)
2. [Helper Utilities](#helper-utilities)
3. [Data Models](#data-models)
4. [Constants](#constants)

---

## Service APIs

### CarpoolService

Service for managing carpool operations including driver registration, offer creation, and passenger management.

#### RegisterDriverAsync

Registers a new driver in the carpool system.

**Signature**: `Task<(bool Success, string Message, Driver? Driver)> RegisterDriverAsync(int userId, string driverLicenseNumber, string province, string licensePlate, int capacity, VehicleType vehicleType, DriverType driverType, string? accessibilityFeatures = null)`

**Parameters**:
- `userId`: ID of the user registering as driver
- `driverLicenseNumber`: Driver's license number (encrypted)
- `province`: Province/territory code
- `licensePlate`: License plate number (encrypted)
- `capacity`: Maximum number of passengers
- `vehicleType`: Type of vehicle
- `driverType`: Type of driver (Student/Organizer)
- `accessibilityFeatures`: Optional accessibility features

**Returns**: Tuple with success status, message, and driver object

**Throws**: `ArgumentException` if validation fails

#### CreateOfferAsync

Creates a new carpool offer for an event.

**Signature**: `Task<(bool Success, string Message, CarpoolOffer? Offer)> CreateOfferAsync(int eventId, int driverId, int seatsAvailable, string departureInfo, string? departureAddress, double? latitude, double? longitude, DateTime departureTime)`

**Parameters**:
- `eventId`: ID of the event
- `driverId`: ID of the driver
- `seatsAvailable`: Number of available seats
- `departureInfo`: Departure location description
- `departureAddress`: Optional street address
- `latitude`: Optional latitude coordinate
- `longitude`: Optional longitude coordinate
- `departureTime`: Scheduled departure time

**Returns**: Tuple with success status, message, and offer object

#### JoinOfferAsync

Allows a passenger to join a carpool offer.

**Signature**: `Task<(bool Success, string Message)> JoinOfferAsync(int offerId, int passengerId)`

**Parameters**:
- `offerId`: ID of the carpool offer
- `passengerId`: ID of the passenger joining

**Returns**: Tuple with success status and message

---

### RoomRentalService

Service for managing room rental operations including room creation, rental requests, and approvals.

#### CreateRoomAsync

Creates a new room available for rental.

**Signature**: `Task<(bool Success, string Message, Room? Room)> CreateRoomAsync(int organizerId, string name, string address, string? roomInfo, int capacity, decimal hourlyRate, DateTime availabilityStart, DateTime availabilityEnd, string? amenities = null)`

**Parameters**:
- `organizerId`: ID of the organizer creating the room
- `name`: Room name
- `address`: Room address
- `roomInfo`: Optional room description
- `capacity`: Maximum capacity
- `hourlyRate`: Hourly rental rate
- `availabilityStart`: Start of availability period
- `availabilityEnd`: End of availability period
- `amenities`: Optional comma-separated amenities

**Returns**: Tuple with success status, message, and room object

#### RequestRentalAsync

Requests a room rental.

**Signature**: `Task<(bool Success, string Message, RoomRental? Rental)> RequestRentalAsync(int roomId, int renterId, DateTime startTime, DateTime endTime, string purpose, int expectedAttendees)`

**Parameters**:
- `roomId`: ID of the room
- `renterId`: ID of the renter
- `startTime`: Rental start time
- `endTime`: Rental end time
- `purpose`: Purpose of rental
- `expectedAttendees`: Expected number of attendees

**Returns**: Tuple with success status, message, and rental object

#### ApproveRentalAsync

Approves a pending room rental.

**Signature**: `Task<(bool Success, string Message)> ApproveRentalAsync(int rentalId, int organizerId)`

**Parameters**:
- `rentalId`: ID of the rental
- `organizerId`: ID of the approving organizer

**Returns**: Tuple with success status and message

---

### NotificationService

Service for managing user notifications.

#### CreateNotificationAsync

Creates a new notification for a user.

**Signature**: `Task<(bool Success, string Message, Notification? Notification)> CreateNotificationAsync(int userId, NotificationType type, string title, string message, int? relatedEntityId = null, string? relatedEntityType = null, string? actionUrl = null)`

**Parameters**:
- `userId`: ID of the user to notify
- `type`: Type of notification
- `title`: Notification title
- `message`: Notification message
- `relatedEntityId`: Optional related entity ID
- `relatedEntityType`: Optional related entity type
- `actionUrl`: Optional action URL

**Returns**: Tuple with success status, message, and notification object

#### GetUnreadNotificationsAsync

Gets all unread notifications for a user.

**Signature**: `Task<List<Notification>> GetUnreadNotificationsAsync(int userId)`

**Parameters**:
- `userId`: ID of the user

**Returns**: List of unread notifications

#### MarkAsReadAsync

Marks a notification as read.

**Signature**: `Task<(bool Success, string Message)> MarkAsReadAsync(int notificationId, int userId)`

**Parameters**:
- `notificationId`: ID of the notification
- `userId`: ID of the user

**Returns**: Tuple with success status and message

---

### TicketSigningService

Service for signing and verifying ticket QR codes using HMAC-SHA256.

#### SignTicket

Signs a ticket with HMAC-SHA256 signature.

**Signature**: `string SignTicket(int ticketId, int eventId, int userId, string uniqueCode, DateTime expiryTime)`

**Parameters**:
- `ticketId`: ID of the ticket
- `eventId`: ID of the event
- `userId`: ID of the user
- `uniqueCode`: Unique ticket code
- `expiryTime`: Ticket expiry time

**Returns**: Signed token string

#### VerifyTicket

Verifies a signed ticket token.

**Signature**: `TicketValidationResult VerifyTicket(string signedToken)`

**Parameters**:
- `signedToken`: Signed token to verify

**Returns**: Validation result with success status and ticket information

---

### EncryptionService

Service for encrypting and decrypting sensitive data using AES-256.

#### Encrypt

Encrypts a string value.

**Signature**: `string Encrypt(string plainText)`

**Parameters**:
- `plainText`: Text to encrypt

**Returns**: Encrypted string (Base64 encoded)

**Throws**: `Exception` if encryption fails

#### Decrypt

Decrypts an encrypted string.

**Signature**: `string Decrypt(string cipherText)`

**Parameters**:
- `cipherText`: Encrypted text to decrypt

**Returns**: Decrypted string

**Throws**: `Exception` if decryption fails

#### EncryptLicensePlate

Encrypts a license plate number.

**Signature**: `string EncryptLicensePlate(string licensePlate)`

**Parameters**:
- `licensePlate`: License plate to encrypt

**Returns**: Encrypted license plate

#### DecryptLicensePlate

Decrypts a license plate number.

**Signature**: `string DecryptLicensePlate(string encryptedPlate)`

**Parameters**:
- `encryptedPlate`: Encrypted license plate

**Returns**: Decrypted license plate

---

### ProximityService

Service for calculating distances and checking carpool eligibility.

#### CalculateDistance

Calculates distance between two coordinates using Haversine formula.

**Signature**: `double CalculateDistance(double lat1, double lon1, double lat2, double lon2)`

**Parameters**:
- `lat1`: Latitude of first point
- `lon1`: Longitude of first point
- `lat2`: Latitude of second point
- `lon2`: Longitude of second point

**Returns**: Distance in kilometers

#### CheckDriverEligibilityAsync

Checks if a driver is eligible for a carpool offer.

**Signature**: `Task<(bool IsEligible, string? Reason)> CheckDriverEligibilityAsync(int driverId)`

**Parameters**:
- `driverId`: ID of the driver

**Returns**: Tuple with eligibility status and reason if not eligible

#### GetNearbyOffersAsync

Gets carpool offers near a location.

**Signature**: `Task<List<CarpoolOffer>> GetNearbyOffersAsync(int eventId, double latitude, double longitude, double radiusKm = 10.0)`

**Parameters**:
- `eventId`: ID of the event
- `latitude`: Latitude of location
- `longitude`: Longitude of location
- `radiusKm`: Search radius in kilometers (default: 10.0)

**Returns**: List of nearby carpool offers

---

### LicenseValidationService

Service for validating Canadian driver's licenses and license plates.

#### ValidateDriverLicense

Validates a driver's license number.

**Signature**: `ValidationResult ValidateDriverLicense(string licenseNumber, string province)`

**Parameters**:
- `licenseNumber`: License number to validate
- `province`: Province/territory code

**Returns**: Validation result with success status and message

#### ValidateLicensePlate

Validates a license plate number.

**Signature**: `ValidationResult ValidateLicensePlate(string plateNumber, string province)`

**Parameters**:
- `plateNumber`: Plate number to validate
- `province`: Province/territory code

**Returns**: Validation result with success status and message

---

## Helper Utilities

### ValidationHelper

Static utility class for input validation.

#### IsValidEmail

Validates email format.

**Signature**: `bool IsValidEmail(string? email)`

**Returns**: `true` if email format is valid

#### IsValidStudentId

Validates student ID format (9 digits).

**Signature**: `bool IsValidStudentId(string? studentId)`

**Returns**: `true` if student ID format is valid

#### IsValidString

Validates string length.

**Signature**: `bool IsValidString(string? value, int minLength = 1, int? maxLength = null)`

**Returns**: `true` if string is valid

#### IsValidCoordinates

Validates latitude and longitude coordinates.

**Signature**: `bool IsValidCoordinates(double latitude, double longitude)`

**Returns**: `true` if coordinates are valid

---

### DateTimeHelper

Static utility class for date/time operations.

#### FormatForDisplay

Formats a date for user display.

**Signature**: `string FormatForDisplay(DateTime date, bool includeTime = true, string? timezone = null)`

**Returns**: Formatted date string

#### FormatRelativeTime

Formats a date as relative time ("2 hours ago").

**Signature**: `string FormatRelativeTime(DateTime date, string? timezone = null)`

**Returns**: Relative time string

#### IsWithinRange

Checks if a date is within a range.

**Signature**: `bool IsWithinRange(DateTime date, DateTime start, DateTime end)`

**Returns**: `true` if date is within range

---

### PasswordHelper

Static utility class for password operations.

#### HashPassword

Hashes a password using BCrypt.

**Signature**: `string HashPassword(string password)`

**Returns**: Hashed password

#### VerifyPassword

Verifies a password against a hash.

**Signature**: `bool VerifyPassword(string password, string hash)`

**Returns**: `true` if password matches hash

#### ValidatePasswordStrength

Validates password strength.

**Signature**: `(bool IsValid, string? ErrorMessage) ValidatePasswordStrength(string password)`

**Returns**: Tuple with validation status and error message

---

### FormatHelper

Static utility class for data formatting.

#### FormatCurrency

Formats a decimal as currency.

**Signature**: `string FormatCurrency(decimal amount, string currencySymbol = "$")`

**Returns**: Formatted currency string

#### FormatPercentage

Formats a decimal as percentage.

**Signature**: `string FormatPercentage(decimal value, int decimals = 0)`

**Returns**: Formatted percentage string

#### FormatFileSize

Formats a file size in bytes.

**Signature**: `string FormatFileSize(long bytes)`

**Returns**: Formatted file size string (e.g., "1.5 MB")

---

### EmailHelper

Static utility class for email operations.

#### IsValidEmail

Validates email format.

**Signature**: `bool IsValidEmail(string? email)`

**Returns**: `true` if email format is valid

#### NormalizeEmail

Normalizes email (lowercase, trim).

**Signature**: `string NormalizeEmail(string? email)`

**Returns**: Normalized email

#### MaskEmail

Masks email for privacy.

**Signature**: `string MaskEmail(string? email)`

**Returns**: Masked email (e.g., "u***@example.com")

---

### ErrorHandler

Static utility class for error handling.

#### FormatErrorMessage

Formats a user-friendly error message.

**Signature**: `string FormatErrorMessage(string operation, string? details = null)`

**Returns**: Formatted error message

#### LogError

Logs an error with context.

**Signature**: `void LogError(Exception exception, string context, string? additionalInfo = null)`

#### CreateErrorResponse

Creates a standardized error response.

**Signature**: `(bool Success, string Message) CreateErrorResponse(string message, string? code = null)`

**Returns**: Error response tuple

#### SafeExecute

Wraps an operation with error handling.

**Signature**: `T? SafeExecute<T>(Func<T> operation, string errorMessage) where T : class`

**Returns**: Operation result or default value on error

---

## Data Models

### Event

Represents a campus event.

**Properties**:
- `Id`: Event ID
- `Title`: Event title
- `Description`: Event description
- `EventDate`: Event date and time
- `Location`: Event location
- `Capacity`: Maximum capacity
- `TicketsIssued`: Number of tickets issued
- `TicketType`: Type of ticket (Free/Paid)
- `Price`: Ticket price (if paid)
- `Category`: Event category
- `OrganizerId`: ID of organizer
- `OrganizationId`: ID of organization
- `ApprovalStatus`: Approval status

### User

Represents a user in the system.

**Properties**:
- `Id`: User ID
- `Email`: User email
- `PasswordHash`: Hashed password
- `Name`: User name
- `Role`: User role (Student/Organizer/Admin)
- `ApprovalStatus`: Approval status
- `StudentId`: Student ID (if student)
- `OrganizationId`: Organization ID (if organizer)

### Ticket

Represents a ticket for an event.

**Properties**:
- `Id`: Ticket ID
- `EventId`: Event ID
- `UserId`: User ID
- `UniqueCode`: Unique ticket code
- `ClaimedAt`: When ticket was claimed
- `RedeemedAt`: When ticket was redeemed
- `IsRedeemed`: Whether ticket is redeemed

---

## Constants

### Constants.Validation

Validation-related constants.

- `MinPasswordLength`: Minimum password length (8)
- `MaxPasswordLength`: Maximum password length (128)
- `MaxEmailLength`: Maximum email length (256)
- `MaxNameLength`: Maximum name length (200)

### Constants.Proximity

Proximity-related constants.

- `DefaultRadiusKm`: Default search radius (10.0 km)
- `MaxRadiusKm`: Maximum search radius (50.0 km)

### Constants.Pricing

Pricing-related constants.

- `MinHourlyRate`: Minimum hourly rate (0)
- `MaxHourlyRate`: Maximum hourly rate (1000)

---

**Document Version**: 1.0
**Last Updated**: 2025-01-XX
**Maintained By**: Development Team

