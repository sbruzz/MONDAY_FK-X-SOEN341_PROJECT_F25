# Campus Events & Ticketing System

## 🎓 Overview
A **simplified, beginner-friendly** campus events management platform built for SOEN 341. This project uses a straightforward architecture designed for 2nd year students to easily understand and contribute to.

## 🏗️ Simplified Architecture
- **Backend**: ASP.NET Core 9.0 Web API - Direct controller-based approach (no complex services)
- **Frontend**: Next.js 15 + React 19 + TypeScript - Modern UI with Tailwind CSS
- **Database**: SQLite - Simple, file-based database
- **Authentication**: Session-based (simple, no JWT complexity)

## Features

### Student Features
- **Event Discovery**: Browse events with category filters and search
- **Event Management**: Save events to personal calendar
- **Ticket System**: Claim free and paid tickets with QR codes
- **Notifications**: Receive event reminders and updates

### Organizer Features
- **Event Creation**: Create events with full details and media
- **Analytics Dashboard**: Track event performance metrics
- **QR Scanner**: Validate tickets with web camera or file upload
- **Export Tools**: Export attendee lists as CSV

### Administrator Features
- **User Management**: Approve organizer accounts
- **Event Moderation**: Approve/reject event listings
- **Platform Analytics**: Global statistics and trends
- **Organization Management**: Manage organizations and members

### Additional Features
- **Notification System**: Event reminders and system alerts
- **Payment Tracking**: Mock payment system with status tracking
- **Audit Logging**: Track all administrative actions
- **Category System**: 8 predefined event categories

## Tech Stack

### Backend
- ASP.NET Core 9.0 Web API
- Entity Framework Core with SQLite
- BCrypt for password hashing
- QRCoder for QR code generation
- Session-based authentication

### Frontend
- Next.js 15 with App Router
- React 19 with TypeScript
- Tailwind CSS for styling
- Framer Motion for animations
- shadcn/ui components

## Quick Start

### Prerequisites
- .NET 9.0 SDK
- Node.js 18+ and npm

### Running the Application

1. **Clone and setup**:
   ```bash
   git clone <repository-url>
   cd MONDAY_FK-X-SOEN341_PROJECT_F25
   git checkout frontend-integration
   ```

2. **Start both servers**:
   ```bash
   ./dev-start.sh
   ```

3. **Access the application**:
   - Frontend: http://localhost:3000
   - Backend API: http://localhost:5000/api

### Manual Setup

**Backend**:
```bash
# Install dependencies
dotnet restore

# Run database migrations
dotnet ef database update

# Start API server
dotnet run
```

**Frontend**:
```bash
cd frontend
npm install
npm run dev
```

## API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/signup` - User registration

### Events
- `GET /api/events` - List all events
- `POST /api/events` - Create event (organizer)
- `PUT /api/events/{id}` - Update event
- `DELETE /api/events/{id}` - Delete event

### Categories
- `GET /api/categories` - List categories
- `POST /api/categories` - Create category (admin)

### Analytics
- `GET /api/analytics/events/{id}` - Event analytics
- `GET /api/analytics/platform` - Platform statistics

### Notifications
- `GET /api/notifications` - User notifications
- `POST /api/notifications/send-event-reminder` - Send reminders

### QR Scanning
- `POST /api/qrscan/validate` - Validate ticket
- `GET /api/qrscan/events/{id}/scan-summary` - Scan summary

## Database Schema

The application uses a comprehensive database schema with 10 entities:
- **User**: Students, organizers, and administrators
- **Event**: Event details with categories and analytics
- **Ticket**: QR-coded tickets with payment tracking
- **Organization**: Campus organizations with member management
- **Category**: Event categorization system
- **EventAnalytics**: Performance metrics tracking
- **Notification**: User notification system
- **OrganizationMember**: Many-to-many membership with roles
- **AuditLog**: Administrative action tracking
- **QrScanLog**: Ticket validation history

## 📚 Project Documentation

### 📖 READ ME FIRST!
- **[PROJECT_MAP.md](PROJECT_MAP.md)** - **Complete guide to every file and what it does**
- **[SIMPLE_ARCHITECTURE.md](SIMPLE_ARCHITECTURE.md)** - How the simplified architecture works
- **[INTEGRATION_README.md](INTEGRATION_README.md)** - Integration and running guide

### 🎯 Quick Start
```bash
# 1. Run both backend and frontend
./dev-start.sh

# 2. Access the app
# Frontend: http://localhost:3000
# Backend API: http://localhost:5000/api
```

## 👥 Team Members
- **Salvatore Bruzzese** (40112201) - Backend Lead
- **Souleymane Camara** (40183807) - Frontend
- **Dmitrii Cazacu** (40314501) - Backend
- **Jack Di Spirito** (40287812) - Frontend
- **Kevin Ung** (40259218) - Backend, Scrum Master
- **Nand Patel** (40294756) - Frontend Lead

## Development

### Database Migrations
```bash
# Create migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update
```

### Frontend Development
```bash
cd frontend
npm run dev          # Development server
npm run build        # Production build
npm run lint         # Code linting
```

## License
This project is developed for SOEN341 Fall 2025 at Concordia University.

![Concordia University Logo](https://upload.wikimedia.org/wikipedia/fr/9/97/Universit%C3%A9_Concordia_%28logo%29.svg)

