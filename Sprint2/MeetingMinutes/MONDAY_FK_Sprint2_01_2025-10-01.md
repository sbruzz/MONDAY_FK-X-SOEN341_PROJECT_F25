# Sprint 2 Meeting #01 - Sprint Planning

**Date:** 2025-10-01
**Time:** 14:00 - 16:00
**Location:** Discord Voice Channel
**Scrum Master:** Kevin Ung

## Attendees
- [x] Salvatore Bruzzese
- [x] Souleymane Camara
- [x] Dmitrii Cazacu
- [x] Kevin Ung
- [x] Nand Patel

## Absent
- Jack Di Spirito

## Agenda
1. Sprint 1 retrospective review
2. Sprint 2 goals and deliverables
3. User stories prioritization
4. Task breakdown and estimation
5. Task assignments

## Discussion Points

### 1. Sprint 1 Retrospective Summary
- **What went well:**
  - Successfully implemented authentication system
  - Database schema is solid and well-designed
  - Good collaboration between frontend and backend teams
- **What could improve:**
  - Need better communication on API contracts
  - Should do more frequent code reviews
  - Testing should happen earlier, not just at end of sprint

### 2. Sprint 2 Goals
**Sprint Goal:** Implement core ticketing system with QR codes, organizer analytics, and CSV export functionality

**Key Deliverables:**
- US.01: Student ticket claiming with QR code generation
- US.02: Organizer dashboard with analytics
- Enhanced home page with session-based routing
- CSV export for attendee lists

### 3. User Stories Review
Discussed and prioritized user stories:
- **High Priority:**
  - Task.08: Ticket table creation (Foundation for all ticket features)
  - Task.10/11: Student-ticket relationship and QR generation
  - Task.30/31/32: Login and session management improvements
- **Medium Priority:**
  - Task.12/13: Organizer-event relationship and event creation
  - Task.15: Organizer dashboard
  - Task.16/17: CSV export functionality
- **Lower Priority:**
  - Task.14: Event query optimization (can be iterated on)

### 4. Task Assignments
**Backend Team:**
- **Salvatore:** Task.08 (Ticket Table), Task.10 (Student-Ticket relation), Task.11 (Ticket claiming), Task.30 (Login backend)
- **Dmitrii:** Task.14 (Event query), Task.16 (CSV generation)
- **Kevin:** Task.12 (Organizer-Event relation), Task.13 (Event creation frontend)

**Frontend Team:**
- **Souleymane:** Task.15 (Organizer dashboard), Task.29 (Home page)
- **Nand:** Task.32 (Session-based home pages), Task.31 (Login frontend), Task.17 (CSV button)

### 5. Technical Decisions
**QR Code Library:**
- Decided to use QRCoder library (open source, easy integration)
- Will store QR codes as Base64 strings in database
- Each ticket gets unique GUID as code

**CSV Export:**
- Will create DbCSVCommunicator service
- UTF-8 encoding with BOM for Excel compatibility
- Include: Ticket ID, User Name, Email, Claim Date, Redemption Status

**Session Management:**
- Continue with session-based auth (no JWT for now)
- 20-minute timeout
- Implement role-specific home page routing

### 6. Blockers/Issues
- None at this time. Team has clear understanding of tasks.

## Action Items
- [x] Create GitHub issues for all tasks - Kevin - Due: Oct 1
- [x] Set up QRCoder package in project - Salvatore - Due: Oct 2
- [x] Design organizer dashboard mockup - Souleymane - Due: Oct 3
- [x] All team members to update their availability for next 3 weeks - All - Due: Oct 2

## Next Meeting
- **Date:** 2025-10-08
- **Time:** 14:00
- **Agenda:** Mid-sprint check-in, progress updates, blocker discussion

## Notes
- Sprint 2 duration: 3 weeks (Oct 1 - Oct 20)
- Team velocity from Sprint 1: 40 story points completed
- Sprint 2 target: 45-50 story points
- All team members confirmed availability for the sprint
- Daily async standups will continue in Discord #standup channel
