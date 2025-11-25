# Campus Events System - Glossary

## Overview

This glossary defines key terms, concepts, and abbreviations used throughout the Campus Events system documentation and codebase.

## Table of Contents

1. [General Terms](#general-terms)
2. [Technical Terms](#technical-terms)
3. [Domain-Specific Terms](#domain-specific-terms)
4. [Abbreviations](#abbreviations)

---

## General Terms

### Event

A campus event that can be created by organizers, approved by admins, and attended by students. Events have categories, dates, locations, capacities, and ticket types (free or paid).

### Ticket

A digital ticket issued to a user for an event. Tickets have unique codes, can be free or paid, and are validated via QR codes at event entry.

### User

A person using the system. Users can be Students, Organizers, or Admins, each with different roles and permissions.

### Student

A user role representing a student who can browse events, claim tickets, join carpools, and rent rooms.

### Organizer

A user role representing an event organizer who can create events, manage rooms, and coordinate carpools. Organizers require admin approval.

### Admin

A user role with full system access, including event approval, user management, and system configuration.

### Organization

A group or club that organizes events. Organizations are linked to organizer accounts and can have multiple events.

---

## Technical Terms

### Entity Framework Core (EF Core)

The Object-Relational Mapping (ORM) framework used to interact with the SQLite database. Provides LINQ queries, migrations, and change tracking.

### Razor Pages

The ASP.NET Core framework used for building the web application. Combines server-side C# code with HTML markup in `.cshtml` files.

### Dependency Injection (DI)

A design pattern where dependencies are provided to classes rather than created internally. Used throughout the application for service registration and management.

### Session

Server-side storage for user data between HTTP requests. Used for authentication state and user information.

### HMAC-SHA256

A cryptographic hash function used for signing ticket QR codes to prevent tampering and ensure authenticity.

### AES-256

Advanced Encryption Standard with 256-bit keys, used for encrypting sensitive data like driver license numbers and license plates.

### BCrypt

A password hashing algorithm with adaptive work factor, used for securely storing user passwords.

### QR Code

Quick Response code, a two-dimensional barcode used for ticket validation. Contains signed ticket information that can be scanned at event entry.

### Migration

Database schema changes tracked by Entity Framework Core. Migrations allow version control of database structure.

### LINQ

Language Integrated Query, a .NET feature for querying data. Used extensively for database queries and data manipulation.

### Async/Await

C# language features for asynchronous programming. Used throughout the application for non-blocking I/O operations.

---

## Domain-Specific Terms

### Carpool

A shared ride arrangement where a driver offers transportation to an event for multiple passengers.

### Driver

A user who has registered as a driver in the carpool system. Drivers must provide license information and vehicle details.

### Passenger

A user who joins a carpool offer to get a ride to an event.

### Carpool Offer

A driver's offer to provide transportation to an event, including departure location, time, and available seats.

### Room Rental

A booking of a room for a specific time period, typically for events, meetings, or activities.

### Room

A physical space available for rental, managed by an organizer. Rooms have capacity, amenities, and hourly rates.

### Rental Request

A user's request to rent a room for a specific time period. Requires organizer approval.

### Notification

A message sent to a user about system events, such as rental approvals, event updates, or carpool changes.

### Approval Status

The status of an entity (event, user, rental) in the approval workflow. Can be Pending, Approved, or Rejected.

### Proximity

Geographic distance calculation used to match drivers and passengers for carpools based on location.

### Geocoding

The process of converting addresses to geographic coordinates (latitude/longitude) for distance calculations.

### Haversine Formula

A mathematical formula for calculating distances between two points on a sphere (Earth) given their coordinates.

---

## Abbreviations

### API

Application Programming Interface - A set of protocols and tools for building software applications.

### CRUD

Create, Read, Update, Delete - Basic database operations.

### CSV

Comma-Separated Values - A file format for storing tabular data.

### DI

Dependency Injection - A design pattern for managing dependencies.

### EF Core

Entity Framework Core - The ORM framework used in the application.

### HMAC

Hash-based Message Authentication Code - A cryptographic authentication method.

### HTTP

Hypertext Transfer Protocol - The protocol used for web communication.

### HTTPS

HTTP Secure - Encrypted version of HTTP.

### ID

Identifier - A unique identifier for an entity.

### JSON

JavaScript Object Notation - A data interchange format.

### LINQ

Language Integrated Query - A .NET query language.

### LOC

Lines of Code - A metric for code size.

### ORM

Object-Relational Mapping - A technique for mapping objects to database tables.

### QR

Quick Response - A type of barcode.

### REST

Representational State Transfer - An architectural style for web services.

### SQL

Structured Query Language - A language for managing databases.

### SQLite

A lightweight, file-based database engine.

### UI

User Interface - The visual part of the application.

### UTC

Coordinated Universal Time - The primary time standard.

### XSS

Cross-Site Scripting - A security vulnerability.

---

## Related Documentation

For more information on these terms, see:
- [API Documentation](API_DOCUMENTATION.md)
- [Architecture Documentation](ARCHITECTURE_DETAILED.md)
- [Developer Guide](DEVELOPER_GUIDE.md)
- [Security Guide](SECURITY_GUIDE.md)

---

**Document Version**: 1.0
**Last Updated**: 2025-01-XX
**Maintained By**: Development Team

