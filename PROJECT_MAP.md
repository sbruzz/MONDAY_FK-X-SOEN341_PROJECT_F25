# 🗺️ Campus Events Project Map
## Complete Guide to Every File and What It Does

**Last Updated**: After Simplification  
**Purpose**: Help team members understand the entire codebase structure

---

## 📂 Project Overview

This is a **Campus Events & Ticketing System** with:
- **Backend**: ASP.NET Core 9.0 Web API (C#)
- **Frontend**: Next.js 15 + React 19 + TypeScript
- **Database**: SQLite
- **Architecture**: Simple, controller-based (no complex services layer)

---

## 🎯 Core Concept - How It All Works Together

```
User clicks button in Frontend (React)
  ↓
Frontend calls API endpoint (api-service.ts)
  ↓
Request goes to Backend Controller
  ↓
Controller checks authentication (session)
  ↓
Controller queries Database (via AppDbContext)
  ↓
Returns data to Frontend
  ↓
Frontend displays data to User
```

---

## 📁 Backend Structure (ASP.NET Core)

### 🎛️ Controllers/ - The Main API Endpoints

**What are Controllers?** These handle all HTTP requests from the frontend.

#### **AuthController.cs** - Authentication
- **What it does**: Login, signup, logout
- **Key endpoints**:
  - `POST /api/auth/login` - Login user
  - `POST /api/auth/signup` - Create new user
  - `POST /api/auth/logout` - Logout
  - `GET /api/auth/current` - Get current user
- **How to use**: Just call these endpoints from frontend when user signs in/up

#### **EventsController.cs** - Event Management
- **What it does**: Create, read, update, delete events
- **Key endpoints**:
  - `GET /api/events` - Get all events (with filters)
  - `GET /api/events/{id}` - Get one event
  - `POST /api/events` - Create event (organizers only)
  - `PUT /api/events/{id}` - Update event
  - `DELETE /api/events/{id}` - Delete event
- **How to use**: Organizers create events here

#### **CategoriesController.cs** - Event Categories
- **What it does**: Manage event categories (Academic, Social, Sports, etc.)
- **Key endpoints**:
  - `GET /api/categories` - Get all categories
  - `GET /api/categories/{id}` - Get one category
  - `POST /api/categories` - Create category (admins only)
  - `PUT /api/categories/{id}` - Update category
  - `DELETE /api/categories/{id}` - Delete category

#### **AnalyticsController.cs** - Event Analytics
- **What it does**: Track views, saves, ticket sales
- **Key endpoints**:
  - `GET /api/analytics/events/{id}` - Get event analytics
  - `POST /api/analytics/events/{id}/increment-view` - Track view
  - `GET /api/analytics/platform` - Platform stats (admins only)

#### **NotificationsController.cs** - User Notifications
- **What it does**: Send notifications to users
- **Key endpoints**:
  - `GET /api/notifications` - Get user's notifications
  - `POST /api/notifications` - Create notification
  - `POST /api/notifications/{id}/mark-read` - Mark as read

#### **QrScanController.cs** - QR Code Scanning
- **What it does**: Validate tickets with QR codes
- **Key endpoints**:
  - `POST /api/qrscan/validate` - Scan/validate ticket
  - `GET /api/qrscan/events/{id}/scan-summary` - Get scan stats

#### **ErrorController.cs** - Error Handling
- **What it does**: Returns error pages when something breaks

---

### 💾 Models/ - Database Tables

**What are Models?** These define the database structure (like tables in SQL).

#### **User.cs** - Users Table
- Stores: Students, Organizers, Admins
- Has: Email, password, role, approval status
- **Roles**: Student, Organizer, Admin

#### **Event.cs** - Events Table
- Stores: All events on the platform
- Has: Title, description, date, location, capacity, price, category
- **Relations**: Belongs to an Organizer (User), has Category

#### **Ticket.cs** - Tickets Table
- Stores: User tickets for events
- Has: QR code, price, status, payment info
- **Relations**: Belongs to User and Event

#### **Category.cs** - Categories Table
- Stores: Event types (Academic, Social, Sports, etc.)
- Has: Name, description, icon
- **Used by**: Events to categorize them

#### **Organization.cs** - Organizations Table
- Stores: Campus groups (CS Club, Student Union, etc.)
- Has: Name, description, logo
- **Relations**: Many events belong to organizations

#### **EventAnalytics.cs** - Analytics Table
- Stores: Event performance metrics
- Has: Views, saves, tickets sold, revenue
- **Relations**: One per Event

#### **Notification.cs** - Notifications Table
- Stores: User notifications
- Has: Title, message, type, read status
- **Relations**: Belongs to User

#### **SavedEvent.cs** - Saved Events Table
- Stores: Events users have saved to their calendar
- **Relations**: Links User to Event (many-to-many)

#### **OrganizationMember.cs** - Membership Table
- Stores: Which users belong to which organizations
- **Relations**: Links User to Organization (many-to-many)

#### **QrScanLog.cs** - Scan History Table
- Stores: History of QR code scans
- Has: When scanned, by whom, valid/invalid
- **Used by**: Organizers to track attendance

#### **AuditLog.cs** - Audit Trail Table
- Stores: Log of important actions
- Has: Who did what, when, IP address
- **Used by**: Admins for security

#### **Enums.cs** - Constants
- Defines: UserRole, TicketType, ApprovalStatus, etc.
- **Used by**: All controllers and models

---

### 🔌 Data/AppDbContext.cs - Database Connection
- **What it does**: Connects your app to SQLite database
- **How to use**: Don't touch this, it's configured for you
- **Contains**: All database table definitions and relationships

---

### ⚙️ Program.cs - Application Startup
- **What it does**: Starts your app, configures everything
- **Sets up**: Controllers, database, CORS, sessions
- **Don't modify**: Unless you know what you're doing

---

### 🔧 Services/DatabaseSeeder.cs - Seed Data
- **What it does**: Pre-loads database with test data
- **Creates**: Admin user, categories, organizations
- **How to run**: Called automatically on startup

---

### 📂 Migrations/ - Database Version Control
- **What it does**: Tracks database changes
- **Contains**: History of all database modifications
- **Don't touch**: Let .NET handle this

---

## 🎨 Frontend Structure (Next.js + React)

### 📄 app/ - Next.js Pages (User-Facing Pages)

#### **page.tsx** - Landing Page
- **What it shows**: Hero section with video background
- **User types**: Everyone sees this when they visit the site

#### **auth/signin/page.tsx** - Login Page
- **What it does**: Login form
- **Connects to**: `/api/auth/login` endpoint

#### **events/page.tsx** - Events List
- **What it shows**: All events with filters
- **Features**: Search, filter by category/date

#### **events/[id]/page.tsx** - Event Details
- **What it shows**: Single event full details
- **Features**: Claim ticket button, share, save

#### **account/profile/page.tsx** - User Profile
- **What it shows**: User's profile information
- **User types**: All logged-in users

#### **account/saved/page.tsx** - Saved Events
- **What it shows**: Events user saved
- **For**: Students who want to see their saved events

#### **account/tickets/page.tsx** - My Tickets
- **What it shows**: User's purchased tickets
- **Features**: QR codes, ticket details

#### **organizer/page.tsx** - Organizer Dashboard
- **What it shows**: Event management dashboard
- **For**: Organizers to see their events

#### **organizer/events/page.tsx** - My Events
- **What it shows**: List of events I created
- **For**: Organizers

#### **organizer/events/new/page.tsx** - Create Event
- **What it does**: Form to create new event
- **Connects to**: `POST /api/events`

#### **organizer/scanner/page.tsx** - QR Scanner
- **What it does**: Scanner to validate tickets
- **For**: Organizers at event entrance

#### **admin/page.tsx** - Admin Dashboard
- **What it shows**: Platform overview
- **For**: Administrators only

#### **admin/approvals/page.tsx** - Pending Approvals
- **What it does**: Approve organizer accounts
- **For**: Admins

#### **admin/events/page.tsx** - Event Moderation
- **What it does**: Approve/reject events
- **For**: Admins

#### **admin/orgs/page.tsx** - Manage Organizations
- **What it does**: Manage organizations
- **For**: Admins

---

### 🧩 components/ - Reusable React Components

#### **site/** - Site-Wide Components
- **SiteHeader.tsx** - Top navigation bar
- **UserDropdown.tsx** - User menu in header
- **MobileNav.tsx** - Mobile menu

#### **events/** - Event-Related Components
- **EventCard.tsx** - Shows one event in a card
- **FilterBar.tsx** - Filters for event list (category, date, search)

#### **organizer/** - Organizer Components
- **EventForm.tsx** - Form to create/edit events

#### **dashboard/** - Dashboard Components
- **DataTable.tsx** - Table to display data
- **StatCard.tsx** - Card showing a statistic

#### **ui/** - UI Components (shadcn/ui)
- **button.tsx** - Buttons
- **card.tsx** - Cards/boxes
- **input.tsx** - Form inputs
- **dialog.tsx** - Modal popups
- **badge.tsx** - Status badges
- And many more... (avatar, dropdown, select, etc.)

#### **auth/ProtectedRoute.tsx** - Route Protection
- **What it does**: Checks if user is logged in before showing page
- **If not logged in**: Redirects to login

---

### 📚 lib/ - JavaScript Utilities

#### **api-service.ts** - API Calls
- **What it does**: All functions to call backend API
- **Contains**: Functions like `getEvents()`, `login()`, `createEvent()`
- **How to use**: Import and call functions from your components
- **Example**:
  ```typescript
  import { getEvents } from '@/lib/api-service'
  const events = await getEvents()
  ```

#### **auth-context.tsx** - User Authentication State
- **What it does**: Manages logged-in user state globally
- **Provides**: Current user information to all pages
- **Don't modify**: Already configured

#### **utils.ts** - Helper Functions
- **What it does**: Utility functions (format date, format currency, etc.)
- **Contains**: Reusable helper functions

#### **mock-data.ts** - Fake Data (for testing)
- **What it does**: Provides fake event data during development
- **When to use**: When backend is not available

#### **csv-export.ts** - CSV Export
- **What it does**: Downloads data as CSV files
- **Used by**: Organizers to export attendee lists

---

### 🎨 Styles
- **globals.css** - Global styles and Tailwind CSS
- **components.json** - shadcn/ui configuration

### 🔧 Config Files
- **package.json** - Frontend dependencies
- **next.config.ts** - Next.js configuration
- **tsconfig.json** - TypeScript configuration
- **jest.config.js** - Test configuration

---

## 🔄 How Data Flows (Example: Creating an Event)

1. **User action**: Organizer clicks "Create Event" button
2. **Frontend**: `EventForm.tsx` component shows form
3. **User fills form**: Title, description, date, etc.
4. **User submits**: Form calls `createEvent()` from `api-service.ts`
5. **HTTP Request**: POST to `http://localhost:5000/api/events`
6. **Backend**: Request hits `EventsController.CreateEvent()` method
7. **Authentication**: Controller checks if user is logged in and is organizer
8. **Validation**: Checks if all required fields are filled
9. **Database**: Creates new record in `Events` table
10. **Response**: Returns the created event data
11. **Frontend**: Receives response, shows success message
12. **Redirect**: Takes user to event details page

---

## 🎯 User Types & What They Can Do

### 👨‍🎓 Student
- **Can**: Browse events, save events, claim tickets, view their tickets
- **Cannot**: Create events, moderate content

### 📅 Organizer
- **Can**: Everything students can PLUS create/edit events, view analytics, scan tickets
- **Cannot**: Approve other organizers, moderate all events

### 👨‍💼 Admin
- **Can**: Everything PLUS approve organizers, moderate events, view platform stats, manage organizations
- **Cannot**: Nothing! Full access

---

## 📝 Common Tasks & Where to Find Files

### "I want to add a new API endpoint"
- **Create new method** in appropriate `Controllers/*.cs` file
- **Add endpoint call** to `frontend/lib/api-service.ts`
- **Test** using Postman or curl

### "I want to add a new field to events"
- **Modify** `Models/Event.cs` 
- **Add** field in create/update forms (frontend)
- **Run migration**: `dotnet ef migrations add MigrationName`

### "I want to change how login works"
- **Modify** `Controllers/AuthController.cs` login method
- **Frontend** already calls it from `lib/api-service.ts`

### "I want to change the landing page"
- **Modify** `frontend/app/page.tsx`
- **Change** components in that file

### "I want to add a new user role"
- **Modify** `Models/Enums.cs` - add to UserRole enum
- **Update** anywhere that checks for roles

---

## 🚫 What Was Removed (Don't Look For These)

- ❌ `Services/` folder - We simplified by removing services layer
- ❌ `DTOs/` folder - We use simple classes now
- ❌ `Middleware/` folder - No global exception handling
- ❌ `CampusEvents.Tests/` - Test project temporarily removed

**Why removed?** To make the project simpler for 2nd year students. Less files = easier to understand.

---

## 🎓 Quick Reference

### Backend (C#)
- **Controllers**: Handle HTTP requests
- **Models**: Define database tables
- **AppDbContext**: Database connection
- **Program.cs**: App startup configuration

### Frontend (TypeScript/React)
- **app/**: Pages users visit
- **components/**: Reusable UI pieces
- **lib/**: Helper functions and API calls
- **public/**: Images, videos, icons

---

## 🔥 Key Takeaway

**Keep it simple!**
- Controllers do the work
- Models define data structure  
- Frontend calls backend API
- That's it! No complex layers or abstractions.

---

**Questions?** Check `SIMPLE_ARCHITECTURE.md` for more details!

