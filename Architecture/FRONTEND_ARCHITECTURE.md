# Frontend System Architecture
**Campus Events & Ticketing System**
**SOEN 341 Fall 2025 - Team MONDAY_FK**
**Sprint 3 Architecture Documentation**

---

## Table of Contents
1. [Frontend Architecture Overview](#frontend-architecture-overview)
2. [High-Level Frontend Architecture](#high-level-frontend-architecture)
3. [Frontend Component Breakdown](#frontend-component-breakdown)
4. [Page Structure and Organization](#page-structure-and-organization)
5. [Layout System](#layout-system)
6. [UI Components and Patterns](#ui-components-and-patterns)
7. [Styling Architecture](#styling-architecture)
8. [Client-Side Scripting](#client-side-scripting)
9. [Routing and Navigation](#routing-and-navigation)
10. [Responsive Design](#responsive-design)
11. [User Experience Patterns](#user-experience-patterns)

---

## Frontend Architecture Overview

The Campus Events & Ticketing System frontend follows the **Razor Pages (MVPM)** pattern implemented in ASP.NET Core. The frontend is responsible for:

- **User Interface Rendering**: Razor Pages (.cshtml) with server-side rendering
- **User Interaction**: Forms, buttons, file uploads, and interactive components
- **Visual Design**: CSS styling with Bootstrap framework and custom styles
- **Client-Side Behavior**: JavaScript for enhanced interactivity
- **Responsive Design**: Mobile-first approach with breakpoints
- **Role-Based UI**: Different layouts and experiences for Student, Organizer, and Admin

**Architecture Style**: Server-Side Rendered (SSR) with Progressive Enhancement

---

## High-Level Frontend Architecture

![Frontend](https://github.com/user-attachments/assets/14b7cb07-174d-4cb2-a021-24f2a51ae3de)

---

## Frontend Component Breakdown

### 1. Razor Pages Structure

**Purpose**: Server-side rendered pages with embedded C# code

**File Organization**:
```
Pages/
├── Student/          # Student-specific pages
│   ├── Home.cshtml
│   ├── Events.cshtml
│   ├── Tickets.cshtml
│   ├── Saved.cshtml
│   └── EventDetails.cshtml
├── Organizer/        # Organizer-specific pages
│   ├── Home.cshtml
│   ├── Events.cshtml
│   ├── CreateEvent.cshtml
│   ├── QRScanner.cshtml
│   └── EventDetails.cshtml
├── Admin/            # Admin-specific pages
│   ├── Home.cshtml
│   ├── Users.cshtml
│   ├── Events.cshtml
│   ├── Organizations.cshtml
│   └── EditUser.cshtml
└── Shared/           # Shared layouts and components
    ├── _Layout.cshtml
    ├── _StudentLayout.cshtml
    ├── _OrganizerLayout.cshtml
    ├── _AdminLayout.cshtml
    └── _ValidationScriptsPartial.cshtml
```

**Key Features**:
- **Server-Side Rendering**: All pages rendered on server with Razor syntax
- **Page Models**: Each page has corresponding `.cshtml.cs` PageModel class
- **View Imports**: Common directives in `_ViewImports.cshtml`
- **View Start**: Default layout configuration in `_ViewStart.cshtml`

---

### 2. Layout System

**Purpose**: Provide consistent page structure and navigation

**Layout Hierarchy**:
```
┌─────────────────────────────────────────────────────────────┐
│                    _Layout.cshtml (Base)                     │
│  - HTML document structure                                   │
│  - Bootstrap CSS/JS includes                                  │
│  - jQuery includes                                            │
│  - Global navigation bar                                      │
│  - Footer                                                     │
└─────────────────────────────────────────────────────────────┘
                              │
                  ┌────────────┼────────────┐
                  ▼            ▼            ▼
┌──────────────────┐  ┌──────────────┐  ┌──────────────┐
│ _StudentLayout    │  │_OrganizerLayout│  │  _AdminLayout │
│ - Student Nav     │  │ - Organizer Nav │  │ - Admin Nav   │
│ - Student Menu    │  │ - Org Menu     │  │ - Admin Menu  │
│ - Student Theme   │  │ - Org Theme    │  │ - Admin Theme │
└──────────────────┘  └──────────────┘  └──────────────┘
```

**Layout Components**:

#### Base Layout (`_Layout.cshtml`)
- **Header**: Site branding, global navigation
- **Navigation Bar**: Role-based menu items
- **Content Area**: `@RenderBody()` for page content
- **Footer**: Copyright, links
- **Scripts**: jQuery, Bootstrap JS, validation scripts

#### Role-Specific Layouts
- **`_StudentLayout.cshtml`**: Student-specific navigation and menu
- **`_OrganizerLayout.cshtml`**: Organizer dashboard navigation
- **`_AdminLayout.cshtml`**: Admin management navigation

**Layout Features**:
- **Responsive Navigation**: Collapsible menu on mobile
- **Active Page Highlighting**: Current page indicated in navigation
- **Session-Based Rendering**: Navigation items based on user role
- **Consistent Styling**: Unified color scheme (beige/brown theme)

---

### 3. Page Models (PageModel Classes)

**Purpose**: Handle HTTP requests, business logic, and data binding

**Responsibilities**:
- **HTTP Method Handlers**: `OnGet()`, `OnPost()`, `OnPostAsync()`
- **Input Binding**: Bind form data to properties
- **Validation**: Server-side validation with `ModelState`
- **Session Management**: Check user authentication and role
- **Data Loading**: Query database through `AppDbContext`
- **Redirects**: Navigate to appropriate pages based on logic

**Example Structure**:
```csharp
public class StudentHomeModel : PageModel
{
    private readonly AppDbContext _context;
    
    // Properties for page data
    public List<Event> UpcomingEvents { get; set; }
    public string UserName { get; set; }
    
    // Constructor with dependency injection
    public StudentHomeModel(AppDbContext context) { ... }
    
    // HTTP GET handler
    public async Task<IActionResult> OnGetAsync() { ... }
    
    // HTTP POST handler
    public async Task<IActionResult> OnPostAsync(int eventId) { ... }
}
```

---

## Page Structure and Organization

### Student Pages

#### `Student/Home.cshtml`
- **Purpose**: Student dashboard with event recommendations
- **Features**:
  - Upcoming events carousel
  - Saved events section
  - Quick navigation to tickets
- **Layout**: `_StudentLayout.cshtml`

#### `Student/Events.cshtml`
- **Purpose**: Browse all available events
- **Features**:
  - Event cards with filters
  - Category filtering
  - Search functionality
- **Layout**: `_StudentLayout.cshtml`

#### `Student/Tickets.cshtml`
- **Purpose**: Display student's claimed tickets
- **Features**:
  - QR code display for each ticket
  - Event details
  - Ticket status (redeemed/not redeemed)
- **Layout**: `_StudentLayout.cshtml`

#### `Student/Saved.cshtml`
- **Purpose**: Show saved events (calendar)
- **Features**:
  - List of saved events
  - Event details with save/unsave toggle
- **Layout**: `_StudentLayout.cshtml`

### Organizer Pages

#### `Organizer/Home.cshtml`
- **Purpose**: Organizer dashboard with analytics
- **Features**:
  - Metric cards (total events, tickets sold, attendance rate)
  - Upcoming events timeline
  - Recent activity feed
  - Most popular event highlight
- **Layout**: `_OrganizerLayout.cshtml`

#### `Organizer/CreateEvent.cshtml`
- **Purpose**: Event creation form
- **Features**:
  - Multi-step form with validation
  - Date/time pickers
  - Category selection
  - Capacity settings
- **Layout**: `_OrganizerLayout.cshtml`

#### `Organizer/QRScanner.cshtml`
- **Purpose**: QR code validation for ticket redemption
- **Features**:
  - File upload for QR code images
  - QR validation result display
  - Recent check-ins list
  - Ticket redemption tracking
- **Layout**: `_OrganizerLayout.cshtml`

### Admin Pages

#### `Admin/Home.cshtml`
- **Purpose**: Global analytics dashboard
- **Features**:
  - Global statistics (total events, users, tickets)
  - Pending approvals count
  - Trends and analytics charts
  - Events by category breakdown
- **Layout**: `_AdminLayout.cshtml`

#### `Admin/Users.cshtml`
- **Purpose**: User management and approval
- **Features**:
  - User list with filters
  - Approval/rejection actions
  - User role management
  - User search
- **Layout**: `_AdminLayout.cshtml`

#### `Admin/Events.cshtml`
- **Purpose**: Event moderation
- **Features**:
  - Event list with approval status
  - Approve/reject actions
  - Event details view
  - Event search and filters
- **Layout**: `_AdminLayout.cshtml`

---

## Styling Architecture

### CSS Organization

```
wwwroot/css/
├── bootstrap.min.css      # Bootstrap 5 framework
├── site.css              # Custom site-wide styles
└── [page-specific].css   # Page-specific styles (if needed)
```

### Styling Strategy

**1. Bootstrap Framework (v5.x)**
- **Grid System**: Responsive 12-column grid
- **Components**: Buttons, forms, cards, modals, tables
- **Utilities**: Spacing, colors, display utilities
- **Customization**: Overridden with custom CSS variables

**2. Custom CSS Variables**
```css
:root {
    --primary-color: #8B7355;      /* Beige/brown primary */
    --secondary-color: #D4C5B9;    /* Light beige */
    --accent-color: #6B5B47;      /* Dark brown */
    --text-color: #333333;
    --background-color: #F5F5F5;
}
```

**3. Component Styles**
- **Cards**: Event cards, metric cards, dashboard cards
- **Forms**: Input fields, buttons, validation states
- **Tables**: Responsive tables with horizontal scroll on mobile
- **Modals**: Confirmation dialogs, information modals
- **Navigation**: Sticky navigation, mobile menu

**4. Responsive Breakpoints**
```css
/* Mobile First Approach */
@media (min-width: 576px) { /* Small devices */ }
@media (min-width: 768px) { /* Tablets */ }
@media (min-width: 992px) { /* Desktops */ }
@media (min-width: 1200px) { /* Large desktops */ }
```

**5. Color Scheme**
- **Primary**: Beige/Brown (#8B7355)
- **Secondary**: Light Beige (#D4C5B9)
- **Accent**: Dark Brown (#6B5B47)
- **Success**: Green (#28a745)
- **Danger**: Red (#dc3545)
- **Info**: Blue (#17a2b8)

---

## Client-Side Scripting

### JavaScript Libraries

**1. jQuery (v3.x)**
- **Purpose**: DOM manipulation and event handling
- **Usage**: Form interactions, AJAX calls, dynamic content

**2. Bootstrap JavaScript**
- **Purpose**: Component functionality (modals, dropdowns, tooltips)
- **Usage**: Modal dialogs, navigation toggles

**3. Validation Scripts**
- **Purpose**: Client-side form validation
- **Usage**: Real-time validation feedback before form submission

### Custom JavaScript

**File Organization**:
```
wwwroot/js/
├── site.js              # Site-wide JavaScript
└── [page-specific].js   # Page-specific scripts (if needed)
```

**Common JavaScript Patterns**:

#### 1. Form Validation
```javascript
// Client-side validation before submission
$('#eventForm').on('submit', function(e) {
    if (!validateForm()) {
        e.preventDefault();
        showValidationErrors();
    }
});
```

#### 2. File Upload Handling
```javascript
// QR code file upload
$('#qrFileInput').on('change', function() {
    const file = this.files[0];
    if (file) {
        validateFile(file);
        previewFile(file);
    }
});
```

#### 3. Loading States
```javascript
// Show loading spinner during form submission
$('#submitButton').on('click', function() {
    $(this).prop('disabled', true);
    $('#loadingSpinner').show();
});
```

#### 4. Dynamic Content Updates
```javascript
// Update ticket list without page reload
function updateTicketList() {
    $.ajax({
        url: '/Student/Tickets',
        success: function(data) {
            $('#ticketList').html(data);
        }
    });
}
```

---

## Routing and Navigation

### URL Structure

**Student Routes**:
- `/Student/Home` - Student dashboard
- `/Student/Events` - Browse events
- `/Student/Tickets` - My tickets
- `/Student/Saved` - Saved events
- `/Student/EventDetails/{id}` - Event details

**Organizer Routes**:
- `/Organizer/Home` - Organizer dashboard
- `/Organizer/Events` - My events
- `/Organizer/CreateEvent` - Create event
- `/Organizer/QRScanner/{eventId}` - QR scanner
- `/Organizer/EventDetails/{id}` - Event details

**Admin Routes**:
- `/Admin/Home` - Admin dashboard
- `/Admin/Users` - User management
- `/Admin/Events` - Event moderation
- `/Admin/Organizations` - Organization management
- `/Admin/EditUser/{id}` - Edit user

**Public Routes**:
- `/` - Landing page
- `/Login` - Login page
- `/Signup` - Signup selection
- `/SignupStudent` - Student registration
- `/SignupOrganizer` - Organizer registration
- `/Events` - Public events browsing
- `/EventDetails/{id}` - Public event details

### Navigation Flow

```
┌─────────────┐
│   Landing   │
│    Page     │
└──────┬──────┘
       │
       ▼
┌─────────────┐
│   Login     │──────────────┐
│   /Signup   │              │
└──────┬──────┘              │
       │                      │
       ▼                      │
┌─────────────┐               │
│  Role-Based │               │
│   Redirect  │               │
└──────┬──────┘               │
       │                      │
   ┌───┴───┬──────────┐       │
   ▼       ▼          ▼       │
┌─────┐ ┌──────┐ ┌──────┐     │
│Stud │ │Organ │ │Admin │     │
│Home │ │Home  │ │Home  │     │
└─────┘ └──────┘ └──────┘     │
       │                      │
       └──────────────────────┘
         (Session Check)
```

### Session-Based Routing

**Pattern**: Check user session on page load
```csharp
public async Task<IActionResult> OnGetAsync()
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
    {
        return RedirectToPage("/Login");
    }
    
    var userRole = HttpContext.Session.GetString("UserRole");
    // Load role-specific data
    return Page();
}
```

---

## Responsive Design

### Mobile-First Approach

**Breakpoints**:
- **Mobile**: < 576px (default)
- **Tablet**: 576px - 991px
- **Desktop**: ≥ 992px

### Responsive Patterns

#### 1. Navigation Menu
- **Desktop**: Horizontal navigation bar
- **Mobile**: Collapsible hamburger menu

#### 2. Event Cards
- **Desktop**: 3-4 cards per row
- **Tablet**: 2 cards per row
- **Mobile**: 1 card per row (stacked)

#### 3. Dashboard Cards
- **Desktop**: Grid layout (3-4 columns)
- **Tablet**: 2 columns
- **Mobile**: Single column (stacked)

#### 4. Tables
- **Desktop**: Full table with all columns
- **Mobile**: Horizontal scroll or card-based layout

#### 5. Forms
- **Desktop**: Multi-column layout
- **Mobile**: Single column, full-width inputs

### Responsive CSS Example

```css
/* Mobile First */
.event-card {
    width: 100%;
    margin-bottom: 1rem;
}

/* Tablet */
@media (min-width: 768px) {
    .event-card {
        width: 48%;
        display: inline-block;
    }
}

/* Desktop */
@media (min-width: 992px) {
    .event-card {
        width: 31%;
    }
}
```

---

## User Experience Patterns

### 1. Loading States

**Pattern**: Show loading indicators during async operations
- **Form Submission**: Disable button, show spinner
- **Page Load**: Show skeleton screens or spinners
- **Data Fetching**: Display loading messages

**Implementation**:
```html
<button type="submit" id="submitBtn">
    <span id="spinner" style="display: none;">Loading...</span>
    Submit
</button>
```

### 2. Error Handling

**Client-Side**:
- **Form Validation**: Real-time error messages
- **File Upload**: File size/type validation
- **Network Errors**: User-friendly error messages

**Server-Side**:
- **Validation Errors**: Display via `ModelState`
- **Error Page**: Generic error page for exceptions
- **User Feedback**: Success/error messages via `TempData`

### 3. Form Validation

**Two-Tier Validation**:
1. **Client-Side**: Immediate feedback (HTML5 + JavaScript)
2. **Server-Side**: Security and data integrity (PageModel validation)

**Validation Features**:
- Required field indicators
- Real-time validation feedback
- Clear error messages
- Success confirmation

### 4. Confirmation Dialogs

**Pattern**: Use modals for destructive actions
- **Delete Operations**: Confirm before deletion
- **Approval Actions**: Confirm approve/reject
- **Form Submission**: Warn about unsaved changes

### 5. File Upload UX

**QR Scanner File Upload**:
- **Drag & Drop**: Visual drop zone
- **File Preview**: Show selected file name
- **Progress Indicator**: Show upload progress
- **Validation Feedback**: Clear success/error messages

### 6. Accessibility

**Features**:
- **Semantic HTML**: Proper heading hierarchy
- **ARIA Labels**: Screen reader support
- **Keyboard Navigation**: Tab order, keyboard shortcuts
- **Color Contrast**: WCAG AA compliance
- **Alt Text**: Images have descriptive alt text

---

## Component Examples

### Event Card Component

```html
<div class="card event-card">
    <div class="card-body">
        <h5 class="card-title">@event.Title</h5>
        <p class="card-text">@event.Description</p>
        <div class="event-meta">
            <span class="date">@event.Date.ToString("MMM dd, yyyy")</span>
            <span class="category">@event.Category</span>
        </div>
        <a href="/Student/EventDetails/@event.Id" class="btn btn-primary">View Details</a>
    </div>
</div>
```

### Metric Card Component

```html
<div class="metric-card">
    <div class="metric-value">@TotalEvents</div>
    <div class="metric-label">Total Events</div>
    <div class="metric-trend">
        <span class="trend-up">+12%</span> from last month
    </div>
</div>
```

### Form Component

```html
<form method="post">
    <div class="form-group">
        <label asp-for="Event.Title"></label>
        <input asp-for="Event.Title" class="form-control" />
        <span asp-validation-for="Event.Title" class="text-danger"></span>
    </div>
    <button type="submit" class="btn btn-primary">Create Event</button>
</form>
```

---

## Performance Considerations

### 1. Server-Side Rendering
- **Benefit**: Initial page load with all content
- **Optimization**: Minimize database queries in PageModel

### 2. CSS Optimization
- **Minification**: Use minified CSS files
- **Critical CSS**: Inline critical CSS for above-the-fold content

### 3. JavaScript Optimization
- **Defer Loading**: Load non-critical scripts with `defer`
- **Minification**: Use minified JavaScript files
- **Lazy Loading**: Load components on demand

### 4. Image Optimization
- **QR Codes**: Generate QR codes efficiently
- **Lazy Loading**: Load images as user scrolls

### 5. Caching
- **Browser Caching**: Static assets cached by browser
- **Session Caching**: Session data cached server-side

---

## Security Considerations

### 1. Input Validation
- **Client-Side**: User experience (can be bypassed)
- **Server-Side**: Security (required, cannot be bypassed)

### 2. XSS Prevention
- **Razor Encoding**: Automatic HTML encoding in Razor
- **Content Security**: Validate and sanitize user input

### 3. CSRF Protection
- **Form Tokens**: Anti-forgery tokens in forms
- **Validation**: Validate tokens on POST requests

### 4. Session Security
- **HttpOnly Cookies**: Prevent JavaScript access to session cookies
- **Secure Cookies**: Use HTTPS in production
- **Session Timeout**: Automatic session expiration

---

## Future Enhancements

### Potential Improvements
1. **Progressive Web App (PWA)**: Offline support, push notifications
2. **Client-Side Framework**: React/Vue for enhanced interactivity
3. **Real-Time Updates**: SignalR for live updates
4. **Advanced Animations**: CSS animations library
5. **Component Library**: Reusable UI component system

---

**Document Version**: 1.0  
**Last Updated**: Sprint 3 (November 2025)  
**Maintained By**: Team MONDAY_FK

