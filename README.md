
# Campus Events & Ticketing System

**SOEN 341 Fall 2025 - Course Project**

A comprehensive web application for managing campus events with integrated ticketing, built using ASP.NET Core and Razor Pages.

## Overview

Campus events are an integral part of university life, bringing the community together to learn and connect. This project streamlines event organization at scale by providing tailored tools for three distinct user types: students, organizers, and administrators.

## Features

### Student Experience
- **Event Discovery**: Browse and search events with filters (date, category, organization)
- **Personal Calendar**: Save interesting events to a personalized calendar
- **Ticketing**: Claim free tickets or purchase paid tickets (mock payment)
- **QR Codes**: Receive unique QR code tickets for event entry

### Organizer Tools
- **Event Creation**: Set up events with full details (title, description, date/time, location, capacity, pricing)
- **Analytics Dashboard**: View per-event statistics including:
  - Tickets issued and redeemed
  - Remaining capacity
  - Attendance rates
- **Attendee Management**:
  - Export attendee lists as CSV files
  - Validate QR code tickets via file upload

### Administrator Dashboard
- **Account Approval**: Review and approve organizer account requests
- **Event Moderation**: Approve or reject event listings before publication
- **Global Analytics**: Monitor platform-wide metrics including:
  - Total events and tickets issued
  - Participation trends across organizations
  - Attendance patterns

## Technologies

- **Framework**: [ASP.NET Core 9.0](https://dotnet.microsoft.com/en-us/apps/aspnet) with Razor Pages
- **Database**: SQLite with Entity Framework Core
- **Authentication**: Session-based with BCrypt password hashing
- **QR Codes**: [QRCoder](https://github.com/codebude/QRCoder) library
- **Additional**: JWT support for potential API development

## Getting Started

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- Any modern IDE (Visual Studio, VS Code, Rider)

### Installation

1. Clone the repository:
```bash
git clone https://github.com/sbruzz/MONDAY_FK-X-SOEN341_PROJECT_F25.git
cd MONDAY_FK-X-SOEN341_PROJECT_F25
```

2. Restore dependencies:
```bash
dotnet restore CampusEvents.csproj
```

3. Apply database migrations:
```bash
dotnet ef database update
```

4. Run the application:
```bash
dotnet run
```

5. Open your browser and navigate to:
   - HTTP: `http://localhost:5136`
   - HTTPS: Use `dotnet run --launch-profile https` then visit `https://localhost:7295`

### Development Mode

For development with hot reload:
```bash
dotnet watch run
```

## Project Structure

```
├── Pages/              # Razor Pages (UI + Page Models)
│   ├── Student/       # Student-facing pages
│   ├── Organizer/     # Organizer event management
│   └── Admin/         # Administrator dashboard
├── Models/            # Domain entities (Event, User, Ticket, etc.)
├── Data/              # Database context and services
├── Migrations/        # EF Core migration history
├── wwwroot/           # Static files (CSS, JS, images)
└── Program.cs         # Application entry point and configuration
```

## Development Process

This project follows **Agile Scrum** methodology with 4 sprints (~3-4 weeks each).

### GitHub Workflow
- **User Stories**: Tracked as issues with prefix `US.##`
- **Tasks**: Derived from user stories with prefix `Task.##`
- **Branches**: Active development on `develop`, PRs to `main`
- **Documentation**: Sprint deliverables and meeting minutes in dedicated folders

## Team Members

| Name | Student ID | GitHub | Role |
|------|-----------|--------|------|
| Sana Asgharpour | 40244364 | - | - |
| Salvatore Bruzzese | 40112201 | sbruzz | Backend |
| Souleymane Camara | 40183807 | mistersuun | Frontend |
| Dmitrii Cazacu | 40314501 | Hildthelsta | Backend |
| Jack Di Spirito | 40287812 | JackDiSpirito | Frontend |
| Kevin Ung | 40259218 | pengukev | Backend (Scrum Master) |
| Nand Patel | 40294756 | ns-1456 | Frontend |

## Utility Classes and Helpers

The project includes a comprehensive set of utility classes in the `Services/` directory:

- **ValidationHelper**: Input validation, email/student ID validation, coordinate validation
- **DateTimeHelper**: Date formatting, relative time, timezone conversion
- **StringExtensions**: String manipulation and formatting extensions
- **ErrorHandler**: Centralized error handling and logging
- **Constants**: Application-wide constants and configuration values
- **QueryHelper**: Database query extension methods and filtering
- **PasswordHelper**: Secure password hashing and validation
- **EmailHelper**: Email validation and formatting utilities
- **FormatHelper**: Currency, percentage, file size, and phone number formatting
- **ServiceExtensions**: Dependency injection extension methods
- **CacheHelper**: In-memory caching utilities
- **LoggingHelper**: Structured logging helpers
- **ResponseHelper**: Standardized API response creation
- **FileHelper**: File validation and MIME type detection
- **NumberHelper**: Number manipulation and formatting utilities

## Contributing

Please see [CONTRIBUTING.md](CONTRIBUTING.md) for contribution guidelines and workflow.

## License

This project is developed as part of SOEN 341 coursework at Concordia University.

---

![Concordia University](https://upload.wikimedia.org/wikipedia/fr/9/97/Universit%C3%A9_Concordia_%28logo%29.svg)

