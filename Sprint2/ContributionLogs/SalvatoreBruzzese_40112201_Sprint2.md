# Sprint 2 Contribution Log
**Name:** Salvatore Bruzzese
**Student ID:** 40112201
**GitHub Username:** sbruzz
**Role:** Backend Developer

## Week 1 (Sept 30 - Oct 6, 2025)

### Activities

| Date | Activity | Description | Time Spent | Related Issue/Task |
|------|----------|-------------|------------|-------------------|
| Oct 1 | Team meeting | Sprint 2 planning and task assignment | 2 hours | - |
| Oct 2 | Database modeling | Designed Ticket entity model with QR code field | 3 hours | Task.08 |
| Oct 3 | Backend development | Created Ticket table with EF Core migration | 4 hours | Task.08 |
| Oct 4 | Testing | Tested ticket table creation and relationships | 2 hours | Task.08 |
| Oct 5 | Code review | Reviewed event creation backend | 1 hour | Task.12 |

**Total Time This Week:** 12 hours

---

## Week 2 (Oct 7 - Oct 13, 2025)

### Activities

| Date | Activity | Description | Time Spent | Related Issue/Task |
|------|----------|-------------|------------|-------------------|
| Oct 8 | Team meeting | Mid-sprint check-in | 1.5 hours | - |
| Oct 9 | Backend development | Implemented student-ticket relationship logic | 5 hours | Task.10 |
| Oct 10 | QR code integration | Integrated QRCoder library for ticket generation | 4 hours | Task.10 |
| Oct 11 | Backend development | Implemented ticket claim handler with capacity check | 4 hours | Task.11 |
| Oct 12 | Testing | End-to-end testing of ticket claiming flow | 2.5 hours | Task.11 |

**Total Time This Week:** 17 hours

---

## Week 3 (Oct 14 - Oct 20, 2025)

### Activities

| Date | Activity | Description | Time Spent | Related Issue/Task |
|------|----------|-------------|------------|-------------------|
| Oct 15 | Team meeting | Sprint 2 review and retrospective | 2 hours | - |
| Oct 16 | Backend development | Implemented login authentication with BCrypt | 6 hours | Task.30 |
| Oct 17 | Session management | Configured session-based auth in Program.cs | 3 hours | Task.30 |
| Oct 18 | Testing | Tested login flow for all user roles | 2 hours | Task.30 |
| Oct 19 | Bug fixing | Fixed session timeout and redirect issues | 2 hours | Task.30 |
| Oct 20 | Code review | Reviewed frontend session-based home pages | 1 hour | Task.32 |

**Total Time This Week:** 16 hours

---

## Summary

### Key Contributions
- Designed and implemented Ticket entity with full relationships
- Integrated QRCoder library for unique ticket QR code generation
- Built student-ticket relationship with automatic ticket issuance
- Implemented secure login authentication system with BCrypt password hashing
- Configured session management for role-based access control

### Tasks Completed
- Task.08 - [Backend] Ticket Table Creation
- Task.10 - [Backend] Relation between student and tickets
- Task.11 - [Frontend] Ticket claim and QR code display
- Task.30 - [Backend] Login authentication

### Challenges Faced
- **Challenge:** QR code generation causing performance issues with large batch ticket claims
  - **Resolution:** Optimized QR generation by caching QRCodeGenerator instance
- **Challenge:** Session persistence issues after server restart
  - **Resolution:** Documented limitation and planned Redis integration for production
- **Challenge:** Concurrent ticket claims exceeding event capacity
  - **Resolution:** Implemented database-level transaction locking

### Learnings
- Mastered Entity Framework Core relationship configurations (1:N, N:1)
- Learned QRCoder library implementation and Base64 encoding
- Deepened understanding of ASP.NET Core session management
- Improved skills in BCrypt password hashing best practices

---

**Total Time Spent on Sprint 2:** 45 hours
