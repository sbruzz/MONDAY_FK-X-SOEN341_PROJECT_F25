# Sprint 1 Meeting #01 - Sprint Planning

**Date:** 2025-09-09
**Time:** 14:00 - 16:00
**Location:** Discord Voice Channel
**Scrum Master:** Kevin Ung

## Attendees
- [x] Salvatore Bruzzese
- [x] Souleymane Camara
- [x] Dmitrii Cazacu
- [x] Jack Di Spirito
- [x] Kevin Ung
- [x] Nand Patel

## Absent
- Sana Asgharpour

## Agenda
1. Sprint 1 kickoff and project introduction
2. Review project requirements
3. Discuss user stories and task assignments
4. Set up development environment
5. Define team workflow and communication channels

## Discussion Points

### 1. Sprint 1 Goals
**Sprint Goal:** Establish project foundation with database schema, authentication system, and basic user interface.

**Key Deliverables:**
- Database schema design and implementation (ERD)
- User authentication system (login/signup)
- Basic frontend pages (home, login, signup, events)
- Project setup and repository structure

### 2. User Stories Review
Discussed initial user stories:
- **US.01: Student** - Browse events, claim tickets, save events
- **US.02: Organizer** - Create events, manage dashboard
- **US.03: Administrator** - Approve users, moderate events

**Priority:** Foundation tasks first (database, authentication), then user-facing features

### 3. Task Assignments
**Backend Team:**
- **Kevin:** Task.01 (Event schema), Task.02 (Project initialization), Task.08 (ERD design), Task.09 (User model), Task.10 (Event model), Task.15 (EF Core setup), Task.22 (Session config)
- **Salvatore:** Task.11 (Ticket model), Task.13 (SavedEvent model), Task.20 (Login logic), Task.21 (Signup logic), Task.33 (Event queries)
- **Dmitrii:** Task.12 (Organization model), Task.15 (Database migrations), Task.34 (Organization queries)

**Frontend Team:**
- **Souleymane:** Task.18 (Login page), Task.24 (Home page), Task.26 (Events browsing), Task.27 (Event details)
- **Nand:** Task.04 (Shared layouts), Task.19 (Signup pages - Student & Organizer)
- **Jack:** [Initial tasks - will be assigned in follow-up]

### 4. Technical Decisions

**Technology Stack:**
- **Framework:** ASP.NET Core 9.0 (Razor Pages)
- **Database:** SQLite with Entity Framework Core
- **Authentication:** Session-based with BCrypt password hashing
- **Frontend:** Bootstrap 5 for styling

**Database Design:**
- Agreed on ERD with 5 main entities: User, Event, Ticket, Organization, SavedEvent
- User roles: Student, Organizer, Administrator
- Event approval workflow: Pending → Approved/Rejected
- Foreign key relationships clearly defined

**Project Structure:**
- Razor Pages architecture
- Separate folders for Student, Organizer, Admin pages
- Shared layouts for consistent UI
- Models folder for entity classes

**Authentication Approach:**
- Session-based authentication (no JWT for now)
- BCrypt for password hashing
- Role-based access control
- Session timeout: 20 minutes

### 5. Development Workflow

**Git Workflow:**
- Main branch for stable code
- Feature branches for each task
- Pull requests required for code review
- All team members must review before merge

**Communication:**
- Daily async standups in Discord
- Weekly sync meetings (Tuesdays at 14:00)
- Use GitHub issues for task tracking
- Discord for quick questions and coordination

**Code Standards:**
- Follow C# naming conventions
- Add XML comments to public methods
- Consistent Razor Page structure
- Mobile-responsive design required

### 6. Blockers/Issues

**Initial Setup:**
- Team members need to install .NET 9.0 SDK
- Some members new to ASP.NET Core Razor Pages
- **Resolution:** Kevin will provide setup guide, team will do learning session

**Database Design:**
- Need to finalize ERD before implementation
- **Resolution:** Kevin will create ERD draft for team review by Sept 11

## Action Items
- [x] Create GitHub repository with proper structure - Kevin - Due: Sept 10
- [x] Install .NET 9.0 SDK - All - Due: Sept 10
- [x] Review project requirements document - All - Due: Sept 10
- [x] Finalize ERD design - Kevin - Due: Sept 11
- [x] Set up development environment - All - Due: Sept 11
- [x] Create initial user stories in GitHub - Kevin - Due: Sept 11
- [x] Break down tasks from user stories - All - Due: Sept 12

## Next Meeting
- **Date:** 2025-09-16
- **Time:** 14:00
- **Agenda:** Mid-sprint check-in and progress review

## Notes
- Team is excited to start the project
- Good mix of experience levels - will pair program when needed
- Agreed to help each other learn ASP.NET Core Razor Pages
- Set up GitHub project board for Sprint 1
- Will use GitHub Actions for CI/CD (to be configured in Sprint 1)

---

**Meeting Adjourned:** 16:00

