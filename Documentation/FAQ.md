# Campus Events System - Frequently Asked Questions (FAQ)

## Overview

This document answers common questions about the Campus Events system for users, developers, and administrators.

## Table of Contents

1. [General Questions](#general-questions)
2. [Student Questions](#student-questions)
3. [Organizer Questions](#organizer-questions)
4. [Administrator Questions](#administrator-questions)
5. [Technical Questions](#technical-questions)
6. [Troubleshooting](#troubleshooting)

---

## General Questions

### What is the Campus Events System?

The Campus Events System is a comprehensive web application for managing campus events, ticketing, carpool coordination, and room rentals. It serves students, event organizers, and administrators.

### Who can use the system?

- **Students**: Can browse events, claim tickets, join carpools, and rent rooms
- **Organizers**: Can create events, manage rooms, offer carpools, and validate tickets
- **Administrators**: Can approve users/events, manage drivers, and monitor the system

### How do I create an account?

Navigate to the signup page and select either "Sign up as Student" or "Sign up as Organizer". Fill in the required information and submit. Organizer accounts require admin approval.

### What are the password requirements?

- Minimum 8 characters
- Maximum 128 characters
- Must contain at least one letter
- Must contain at least one number

### How do I reset my password?

Currently, password reset functionality is not implemented. Contact support for assistance.

### Is my data secure?

Yes. The system uses:
- BCrypt password hashing
- AES-256 encryption for sensitive data
- HTTPS for secure communication
- Session-based authentication
- Role-based access control

---

## Student Questions

### How do I claim a ticket?

1. Navigate to the event details page
2. Click "Claim Ticket" (for free events) or "Purchase Ticket" (for paid events)
3. Your ticket will be added to your account
4. Download the QR code for event entry

### Can I claim multiple tickets for the same event?

No. Each user can only claim one ticket per event to ensure fair distribution.

### What happens if an event is at capacity?

You'll see a message indicating the event is full. You can still save the event to your calendar, but you won't be able to claim a ticket until capacity becomes available.

### How do I view my tickets?

Navigate to "My Tickets" from the menu. You'll see all your tickets with QR codes and redemption status.

### How do QR codes work?

QR codes contain a signed token that includes:
- Event ID
- Ticket ID
- Unique ticket code
- Expiry timestamp (24 hours after event)

The QR code is validated at the event using the organizer's QR scanner.

### Can I transfer my ticket to someone else?

Currently, ticket transfers are not supported. Tickets are non-transferable.

### How do I join a carpool?

1. Navigate to the event details page
2. View available carpool offers
3. Click "Join Carpool" on your preferred offer
4. Optionally provide pickup location and notes
5. Confirm your participation

### How do I become a driver?

1. Navigate to "Become a Driver" from the menu
2. Fill in your driver information:
   - Vehicle capacity (1-50 passengers)
   - Vehicle type
   - Driver's license number
   - Province/territory
   - License plate (optional)
3. Submit registration
4. Wait for admin approval

### How long does driver approval take?

Driver approval is typically processed within 24-48 hours. You'll receive a notification when your driver registration is approved or rejected.

### Can I offer a ride for multiple events?

Yes. Once approved as a driver, you can create carpool offers for any event. However, you can only have one active offer per event.

### How do I request a room rental?

1. Navigate to "Rooms" from the menu
2. Browse available rooms
3. Click on a room to view details
4. Click "Request Rental"
5. Fill in rental information (start time, end time, purpose, attendees)
6. Submit request
7. Wait for organizer/admin approval

### How long does room rental approval take?

Approval time varies by organizer. Most requests are processed within 24-48 hours. You'll receive a notification when your request is approved or rejected.

### Can I cancel a room rental?

Yes. You can cancel pending or approved rentals from "My Rentals". Cancelled rentals free up the time slot for other users.

---

## Organizer Questions

### How do I create an event?

1. Navigate to "Create Event" from the menu
2. Fill in event details:
   - Title, description, date, location
   - Capacity, ticket type, price (if paid)
   - Category, organization
3. Submit event
4. Wait for admin approval

### How long does event approval take?

Event approval is typically processed within 24-48 hours. You'll receive a notification when your event is approved or rejected.

### Can I edit an event after creation?

Yes. Navigate to your event details page and click "Edit Event". Note that significant changes may require re-approval.

### How do I see who's attending my event?

1. Navigate to your event details page
2. View event statistics (tickets issued, redeemed, etc.)
3. Click "Export Attendees" to download a CSV file with attendee user IDs

### How do I validate tickets at the event?

1. Navigate to "QR Scanner" from the menu
2. Upload QR code image or scan code
3. System validates ticket and marks as redeemed
4. View validation results

### Can I validate tickets in bulk?

Yes. Upload multiple QR code images at once. The system will validate all tickets and provide a summary report.

### How do I create a room?

1. Navigate to "My Rooms" from the menu
2. Click "Create Room"
3. Fill in room details (name, address, capacity, amenities, rate)
4. Submit room
5. Room is immediately available for rental requests

### How do I approve room rental requests?

1. Navigate to "My Rooms" from the menu
2. Click on a room to view details
3. View pending rental requests
4. Click "Approve" or "Reject" for each request
5. Provide rejection reason if rejecting

### Can I disable a room?

Yes. As an organizer, you can update room status. However, only administrators can permanently disable rooms (which overrides all rentals).

### How do I offer a carpool ride?

1. Register as a driver (if not already)
2. Navigate to your event details page
3. Click "Offer a Ride"
4. Fill in departure information
5. Submit offer
6. Passengers can now join your ride

### Can I see who's in my carpool?

Yes. Navigate to "My Carpools" to see all your carpool offers and their passengers.

### Can I cancel a carpool offer?

Yes. You can cancel offers that don't have confirmed passengers. If passengers have confirmed, you should contact them first before cancelling.

---

## Administrator Questions

### How do I approve an organizer account?

1. Navigate to "Users" from the admin menu
2. View pending organizer requests
3. Review organizer information
4. Click "Approve" or "Reject"
5. Provide rejection reason if rejecting

### What information should I review for organizer approval?

- Organization information
- Position/title
- Department
- Contact information
- Any additional verification needed

### How do I approve an event?

1. Navigate to "Events" from the admin menu
2. View pending events
3. Review event details
4. Click "Approve" or "Reject"
5. Provide rejection reason if rejecting

### What should I check when approving events?

- Event content appropriateness
- Date/time validity
- Capacity reasonableness
- Pricing (if paid event)
- Organizer credibility

### How do I approve a driver?

1. Navigate to "Drivers" from the admin menu
2. View pending driver registrations
3. Review driver information:
   - License number (decrypted for admin view)
   - License plate (decrypted for admin view)
   - Vehicle information
   - User information
4. Click "Approve" or "Suspend"

### When should I suspend a driver?

Suspend a driver if:
- Safety concerns reported
- Multiple complaints
- Violation of terms of service
- Suspicious activity

### How do I manage rooms?

1. Navigate to "Rooms" from the admin menu
2. View all rooms
3. Enable/disable rooms as needed
4. View room statistics
5. Manage room rentals

### When should I disable a room?

Disable a room if:
- Maintenance required
- Safety concerns
- Policy violations
- Organizer request

### What analytics are available?

The admin dashboard shows:
- Total events
- Total tickets issued
- Total users by role
- Active carpool offers
- Room rental statistics
- Recent activity

---

## Technical Questions

### What technology is the system built with?

- **Framework**: ASP.NET Core 9.0 with Razor Pages
- **Database**: SQLite (development), SQL Server/PostgreSQL (production)
- **Authentication**: Session-based with BCrypt
- **QR Codes**: QRCoder library with HMAC signing

### Is the system open source?

The system is developed as part of SOEN 341 coursework. Check the repository for licensing information.

### How do I contribute?

See the CONTRIBUTING.md file for contribution guidelines and workflow.

### How do I report bugs?

Report bugs through:
- GitHub Issues (if repository is public)
- Email: support@campusevents.com
- Contact the development team

### How do I request features?

Feature requests can be submitted through:
- GitHub Issues
- Email: support@campusevents.com
- Team meetings

### What browsers are supported?

The system supports:
- Chrome (latest)
- Firefox (latest)
- Edge (latest)
- Safari (latest)

### Is there a mobile app?

Currently, the system is web-based only. A mobile app may be developed in the future.

### How is data backed up?

- Development: Manual backups recommended
- Production: Automated backups should be configured
- Database: Regular backup schedule recommended

### What are the system requirements?

**Server**:
- .NET 9.0 Runtime
- 2GB+ RAM
- 10GB+ disk space
- SQLite or SQL Server/PostgreSQL

**Client**:
- Modern web browser
- JavaScript enabled
- Internet connection

---

## Troubleshooting

### I can't log in

**Possible causes**:
- Incorrect email or password
- Account not approved (organizers)
- Session expired

**Solutions**:
- Verify email and password
- Check if account is approved
- Clear browser cache and cookies
- Try again

### I can't claim a ticket

**Possible causes**:
- Event at capacity
- Already claimed a ticket
- Event date in past
- Event not approved

**Solutions**:
- Check event capacity
- Verify you haven't already claimed
- Check event date
- Verify event is approved

### QR code won't scan

**Possible causes**:
- QR code corrupted
- QR code expired
- Invalid ticket

**Solutions**:
- Regenerate QR code
- Check ticket expiry
- Verify ticket is valid
- Contact support

### Carpool offer not showing

**Possible causes**:
- Driver not approved
- Offer cancelled
- Event not approved
- No seats available

**Solutions**:
- Check driver status
- Verify offer is active
- Check event approval
- Verify seats available

### Room rental request rejected

**Possible causes**:
- Room disabled
- Time slot already booked
- Capacity exceeded
- Outside availability window

**Solutions**:
- Check room status
- Try different time slot
- Verify capacity
- Check availability window

### Notification not received

**Possible causes**:
- Notification system error
- Email delivery issue
- Notification marked as read

**Solutions**:
- Check notification center
- Verify email settings
- Check spam folder
- Contact support

---

## Additional Resources

- **Documentation**: See Documentation/ directory
- **API Documentation**: Documentation/API_DOCUMENTATION.md
- **User Guide**: Documentation/USER_GUIDE.md
- **Developer Guide**: Documentation/DEVELOPER_GUIDE.md
- **Support**: support@campusevents.com

---

**Document Version**: 1.0
**Last Updated**: 2025-01-XX
**Maintained By**: Development Team

