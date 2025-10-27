`# Campus Events & Ticketing System - Full Stack Integration

This repository now contains both the **ASP.NET Core backend** and **Next.js frontend** for the Campus Events platform, integrated to work together seamlessly.

## 🏗️ Architecture Overview

```
┌─────────────────┐    HTTP/REST API    ┌─────────────────┐
│   Next.js       │◄──────────────────►│  ASP.NET Core   │
│   Frontend      │                     │   Backend       │
│   (Port 3000)   │                     │   (Port 5000)   │
└─────────────────┘                     └─────────────────┘
         │                                        │
         │                                        │
    ┌─────────┐                              ┌─────────┐
    │ React   │                              │ SQLite  │
    │ UI      │                              │ Database│
    └─────────┘                              └─────────┘
```

## 📁 Project Structure

```
MONDAY_FK-X-SOEN341_PROJECT_F25/
├── 📁 Controllers/           # ASP.NET Core API Controllers
│   ├── AuthController.cs    # Authentication endpoints
│   └── EventsController.cs  # Event management endpoints
├── 📁 Data/                 # Database context and models
├── 📁 Models/               # Entity models
├── 📁 Pages/                # ASP.NET Core Razor Pages (legacy)
├── 📁 frontend/             # Next.js React Frontend
│   ├── 📁 app/             # Next.js App Router pages
│   ├── 📁 components/      # React components
│   ├── 📁 lib/             # Utilities and API service
│   └── package.json        # Frontend dependencies
├── Program.cs              # ASP.NET Core startup
├── dev-start.sh            # Development script
└── README.md              # This file
```

## 🚀 Quick Start

### Prerequisites
- **.NET 9.0 SDK** - [Download here](https://dotnet.microsoft.com/download)
- **Node.js 18+** - [Download here](https://nodejs.org/)
- **npm** (comes with Node.js)

### One-Command Setup
```bash
# Make the development script executable and run it
chmod +x dev-start.sh
./dev-start.sh
```

This will:
1. ✅ Check prerequisites`
2. 📦 Install frontend dependencies (if needed)
3. 🔧 Start ASP.NET Core backend on `http://localhost:5000`
4. ⚛️ Start Next.js frontend on `http://localhost:3000`

### Manual Setup (Alternative)

#### Backend (ASP.NET Core)
```bash
# Install dependencies and run
dotnet restore
dotnet run --urls="http://localhost:5000"
```

#### Frontend (Next.js)
```bash
# Install dependencies
cd frontend
npm install

# Start development server
npm run dev
```

## 🌐 Access Points

- **🎨 Frontend (Next.js)**: http://localhost:3000
- **🔧 Backend API**: http://localhost:5000/api
- **📊 Backend Pages**: http://localhost:5000 (legacy Razor pages)

## 🔌 API Integration

The frontend communicates with the backend through RESTful APIs:

### Authentication Endpoints
- `POST /api/auth/login` - User login
- `POST /api/auth/signup` - User registration

### Event Endpoints
- `GET /api/events` - List events (with filtering)
- `GET /api/events/{id}` - Get event details
- `POST /api/events` - Create new event
- `PUT /api/events/{id}` - Update event
- `DELETE /api/events/{id}` - Delete event

### Additional Endpoints (To be implemented)
- `/api/tickets/*` - Ticket management
- `/api/saved-events/*` - Saved events
- `/api/organizations/*` - Organization management
- `/api/admin/*` - Admin functions
- `/api/analytics/*` - Analytics and reporting

## 🎨 Frontend Features

### Modern React/Next.js Stack
- **Next.js 15** with App Router
- **React 19** with TypeScript
- **Tailwind CSS** for styling
- **shadcn/ui** components
- **Framer Motion** animations
- **Lucide React** icons

### Key Pages
- **Landing Page** - Modern hero with video background
- **Events Page** - Advanced filtering and search
- **Event Details** - Rich event information
- **User Dashboard** - Personal calendar and tickets
- **Organizer Dashboard** - Event management tools
- **Admin Dashboard** - Platform oversight

## 🔧 Backend Features

### ASP.NET Core 9.0
- **Entity Framework Core** with SQLite
- **Razor Pages** (legacy UI)
- **Web API Controllers** (new API endpoints)
- **Session-based Authentication**
- **BCrypt** password hashing
- **CORS** enabled for frontend

### Database Models
- **User** - Authentication and roles
- **Event** - Event information and management
- **Ticket** - QR code ticketing system
- **Organization** - Campus organizations
- **SavedEvent** - User event saves

## 🔐 Authentication System

### User Roles
- **Student** - Browse events, claim tickets
- **Organizer** - Create events, manage analytics
- **Admin** - Platform oversight, approvals

### Authentication Flow
1. User signs up/logs in through frontend
2. Frontend calls backend API endpoints
3. Backend validates credentials and returns user data
4. Frontend stores user session and redirects based on role

## 🛠️ Development Workflow

### Making Changes

#### Frontend Changes
```bash
cd frontend
# Make your changes to React components
npm run dev  # Hot reload enabled
```

#### Backend Changes
```bash
# Make your changes to C# controllers/models
dotnet run  # Auto-restart on changes
```

### Adding New API Endpoints

1. **Create Controller** in `Controllers/` folder
2. **Add DTOs** for request/response models
3. **Update Frontend** API service in `frontend/lib/api-service.ts`
4. **Test Integration** using the development script

## 📊 Database Management

### Migrations
```bash
# Create new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update
```

### Seed Data
The database includes pre-seeded organizations:
- Student Union
- Computer Science Association
- Athletics Department

## 🎯 Next Steps

### Immediate Tasks
- [ ] Implement remaining API controllers (Tickets, Organizations, Admin)
- [ ] Add proper authentication middleware
- [ ] Connect frontend authentication to backend sessions
- [ ] Implement QR code generation and scanning
- [ ] Add file upload for event images

### Future Enhancements
- [ ] Real-time notifications with SignalR
- [ ] Email notifications
- [ ] Payment integration for paid tickets
- [ ] Mobile app with React Native
- [ ] Advanced analytics dashboard

## 🤝 Team Development

### Frontend Team (Nand, Souleymane, Jack)
- Focus on React components and user experience
- Implement responsive design and animations
- Integrate with backend APIs

### Backend Team (Salvatore, Dmitrii, Kevin)
- Develop API endpoints and business logic
- Implement authentication and authorization
- Database design and optimization

## 📝 Notes

- The **frontend-integration** branch contains all integration work
- **Main branch** remains untouched as requested
- Both frontend and backend can run independently
- CORS is configured to allow frontend-backend communication
- Development script handles both servers automatically

## 🐛 Troubleshooting

### Common Issues

**Port Already in Use**
```bash
# Kill processes on ports 3000 and 5000
lsof -ti:3000 | xargs kill -9
lsof -ti:5000 | xargs kill -9
```

**Frontend Dependencies**
```bash
cd frontend
rm -rf node_modules package-lock.json
npm install
```

**Backend Issues**
```bash
dotnet clean
dotnet restore
dotnet build
```

---

**Built with ❤️ by the SOEN 341 Team**

*This integration maintains the original ASP.NET Core backend while adding a modern React frontend for the best of both worlds.*
