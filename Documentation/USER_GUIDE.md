# Campus Events System - User Guide

## Overview

This guide provides comprehensive instructions for end users of the Campus Events system, covering all features and functionality available to students, organizers, and administrators.

## Table of Contents

1. [Getting Started](#getting-started)
2. [Student Guide](#student-guide)
3. [Organizer Guide](#organizer-guide)
4. [Administrator Guide](#administrator-guide)
5. [Common Tasks](#common-tasks)
6. [Troubleshooting](#troubleshooting)

---

## Getting Started

### Creating an Account

#### Student Account

1. Navigate to the signup page
2. Select "Sign up as Student"
3. Fill in required information:
   - Email address
   - Password (minimum 8 characters, must contain letters and numbers)
   - Full name
   - Student ID (9 digits)
   - Program of study
   - Year of study
   - Phone number (optional)
4. Click "Create Account"
5. You'll be automatically logged in

#### Organizer Account

1. Navigate to the signup page
2. Select "Sign up as Organizer"
3. Fill in required information:
   - Email address
   - Password
   - Full name
   - Organization information
   - Position/title
   - Department
   - Phone number
4. Click "Create Account"
5. **Important**: Your account requires admin approval before you can create events
6. You'll receive a notification when your account is approved

### Logging In

1. Navigate to the login page
2. Enter your email address and password
3. Click "Log In"
4. You'll be redirected to your role-specific home page

### Password Requirements

- Minimum 8 characters
- Maximum 128 characters
- Must contain at least one letter
- Must contain at least one number

---

## Student Guide

### Browsing Events

#### View All Events

1. Navigate to "Events" from the main menu
2. Browse all available events
3. Use filters to narrow down results:
   - Category (Academic, Career, Social, etc.)
   - Date range
   - Organization
   - Search by keywords

#### Event Details

Click on any event to view:
- Full event description
- Date, time, and location
- Organizer information
- Available tickets
- Related carpool offers
- Related room rentals

### Claiming Tickets

#### Free Events

1. Navigate to event details page
2. Click "Claim Ticket"
3. Ticket is immediately added to your account
4. QR code is generated automatically

#### Paid Events

1. Navigate to event details page
2. Click "Purchase Ticket"
3. Complete mock payment process
4. Ticket is added to your account
5. QR code is generated automatically

#### Viewing Your Tickets

1. Navigate to "My Tickets" from the menu
2. View all your tickets
3. Download QR codes for event entry
4. See redemption status

### Saving Events

1. Navigate to event details page
2. Click "Save to Calendar"
3. Event is added to your saved events
4. View saved events from "Saved Events" menu

### Carpool System

#### Joining a Carpool

1. Navigate to event details page
2. View available carpool offers
3. Click "Join Carpool" on desired offer
4. Optionally provide pickup location and notes
5. Confirm your participation

#### Viewing Your Carpools

1. Navigate to "My Carpools" from the menu
2. View carpools where you're a passenger
3. See driver information and departure details
4. Leave a carpool if needed

#### Becoming a Driver

1. Navigate to "Become a Driver" from the menu
2. Fill in driver information:
   - Vehicle capacity (1-50 passengers)
   - Vehicle type (Mini, Sedan, SUV, Van, Bus)
   - Driver's license number (encrypted)
   - Province/territory
   - License plate (optional, encrypted)
   - Accessibility features (optional)
3. Submit registration
4. **Important**: Driver registration requires admin approval
5. You'll receive a notification when approved

#### Offering a Ride

1. Navigate to event details page
2. Click "Offer a Ride"
3. Fill in departure information:
   - Departure details (time, location)
   - Departure address (optional)
   - Coordinates (optional, for proximity matching)
4. Submit offer
5. Passengers can now join your ride

### Room Rental

#### Browsing Available Rooms

1. Navigate to "Rooms" from the menu
2. Browse available rooms
3. Filter by:
   - Capacity requirements
   - Availability dates
   - Amenities

#### Requesting a Room Rental

1. Navigate to room details page
2. Click "Request Rental"
3. Fill in rental information:
   - Start time
   - End time
   - Purpose
   - Expected number of attendees
4. Submit request
5. **Important**: Rental requires organizer/admin approval
6. You'll receive a notification when approved/rejected

#### Viewing Your Rentals

1. Navigate to "My Rentals" from the menu
2. View all your rental requests
3. See approval status
4. Cancel rentals if needed

### Notifications

#### Viewing Notifications

1. Click the notification bell icon in the header
2. View unread notifications
3. Click notification to view details
4. Mark as read or delete

#### Notification Types

- Room rental approvals/rejections
- Room status changes
- Driver approval status
- Event updates

---

## Organizer Guide

### Creating Events

1. Navigate to "Create Event" from the menu
2. Fill in event details:
   - Title (required, max 200 characters)
   - Description (required, max 5000 characters)
   - Event date and time (must be in future)
   - Location (required)
   - Capacity (required, positive integer)
   - Ticket type (Free or Paid)
   - Price (required if Paid)
   - Category (Academic, Career, Social, etc.)
   - Organization (optional)
3. Click "Create Event"
4. **Important**: Events require admin approval before being visible to students
5. You'll receive a notification when approved/rejected

### Managing Events

#### View Your Events

1. Navigate to "My Events" from the menu
2. View all events you've created
3. See approval status
4. View event statistics

#### Event Statistics

For each event, view:
- Total tickets issued
- Tickets redeemed
- Remaining capacity
- Attendance rate

#### Export Attendee List

1. Navigate to event details page
2. Click "Export Attendees"
3. CSV file is downloaded with attendee user IDs

### QR Code Validation

#### Validate Tickets

1. Navigate to "QR Scanner" from the menu
2. Upload QR code image or scan code
3. System validates ticket:
   - Checks signature
   - Verifies expiry
   - Confirms ticket exists
   - Checks if already redeemed
4. Mark ticket as redeemed if valid

#### Batch Validation

1. Upload multiple QR code images
2. System validates all tickets
3. View validation results
4. Export validation report

### Room Management

#### Creating Rooms

1. Navigate to "My Rooms" from the menu
2. Click "Create Room"
3. Fill in room details:
   - Name (required)
   - Address (required)
   - Capacity (required, positive integer)
   - Room information (optional)
   - Amenities (comma-separated)
   - Hourly rate (optional)
   - Availability window (optional)
4. Click "Create Room"
5. Room is immediately available for rental requests

#### Managing Room Rentals

1. Navigate to "My Rooms" from the menu
2. Click on a room to view details
3. View pending rental requests
4. Approve or reject requests
5. View approved rentals

### Carpool System (Organizers)

#### Becoming a Driver

Organizers can register as drivers to offer rides for their events:

1. Navigate to "Drivers" from the menu
2. Click "Register as Driver"
3. Fill in driver information (same as students)
4. Submit registration
5. Wait for admin approval

#### Offering Rides

1. Navigate to your event details page
2. Click "Offer a Ride"
3. Fill in departure information
4. Submit offer
5. Manage passengers from "My Carpools"

---

## Administrator Guide

### User Management

#### Approving Organizers

1. Navigate to "Users" from the admin menu
2. View pending organizer requests
3. Review organizer information
4. Click "Approve" or "Reject"
5. User receives notification

#### Managing Users

- View all users
- Filter by role
- Search by name or email
- View user details
- Edit user information

### Event Management

#### Approving Events

1. Navigate to "Events" from the admin menu
2. View pending events
3. Review event details
4. Click "Approve" or "Reject"
5. Provide rejection reason if rejecting
6. Organizer receives notification

#### Event Moderation

- View all events
- Filter by approval status
- Search events
- View event statistics
- Edit event details (if needed)

### Driver Management

#### Approving Drivers

1. Navigate to "Drivers" from the admin menu
2. View pending driver registrations
3. Review driver information:
   - License number (decrypted for admin view)
   - License plate (decrypted for admin view)
   - Vehicle information
   - User information
4. Click "Approve" or "Suspend"
5. Driver receives notification

#### Managing Drivers

- View all drivers
- Filter by status (Active, Pending, Suspended)
- View driver history
- Suspend/unsuspend drivers
- View flagged drivers

### Room Management

#### Managing Rooms

1. Navigate to "Rooms" from the admin menu
2. View all rooms
3. Enable/disable rooms
4. View room statistics
5. Manage room rentals

#### Room Status

- **Enabled**: Room available for rental
- **Disabled**: Room disabled by admin (overrides all rentals)
- **Under Maintenance**: Temporarily unavailable

### System Analytics

#### Dashboard Overview

View system-wide statistics:
- Total events
- Total tickets issued
- Total users by role
- Active carpool offers
- Room rental statistics
- Recent activity

---

## Common Tasks

### Changing Your Password

1. Navigate to your profile page
2. Click "Change Password"
3. Enter current password
4. Enter new password
5. Confirm new password
6. Click "Update Password"

### Updating Profile Information

1. Navigate to your profile page
2. Edit desired fields
3. Click "Save Changes"

### Contacting Support

For issues or questions:
- Email: support@campusevents.com
- Check FAQ page
- Review documentation

---

## Troubleshooting

### Login Issues

**Problem**: Cannot log in

**Solutions**:
- Verify email and password are correct
- Check if account is approved (organizers)
- Try password reset
- Contact support if issues persist

### Ticket Issues

**Problem**: Cannot claim ticket

**Solutions**:
- Check if event is at capacity
- Verify you haven't already claimed a ticket
- Check if event date is in the future
- Verify event is approved

### Carpool Issues

**Problem**: Cannot join carpool

**Solutions**:
- Check if seats are available
- Verify you're not already in a carpool for this event
- Check if driver is active
- Verify you're not the driver

### Room Rental Issues

**Problem**: Cannot request room rental

**Solutions**:
- Check if room is enabled
- Verify time slot is available
- Check if room is within availability window
- Verify capacity requirements

---

## Tips and Best Practices

### For Students

- Save interesting events to your calendar
- Join carpools early for popular events
- Request room rentals well in advance
- Keep your profile information updated

### For Organizers

- Create events with detailed descriptions
- Set appropriate capacity limits
- Respond to rental requests promptly
- Use QR scanner for efficient ticket validation

### For Administrators

- Review pending requests regularly
- Provide clear rejection reasons
- Monitor system statistics
- Keep security keys secure

---

**Document Version**: 1.0
**Last Updated**: 2025-01-XX
**Maintained By**: Development Team

