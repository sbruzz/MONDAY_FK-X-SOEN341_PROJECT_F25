# Sprint 1 Contribution Log
**Name:** Salvatore Bruzzese
**Student ID:** 40112201
**GitHub Username:** sbruzz
**Role:** Backend Developer

## Week 1 (Sept 9 - Sept 15, 2025)

### Activities

| Date | Activity | Description | Time Spent | Related Issue/Task |
|------|----------|-------------|------------|-------------------|
| Sept 9 | Team meeting | Sprint 1 kickoff and planning | 2 hours | - |
| Sept 10 | Environment setup | Installed .NET 9.0 SDK and tools | 2 hours | - |
| Sept 11 | Repository setup | Cloned repo and reviewed project structure | 1.5 hours | Task.03 |
| Sept 12 | Database modeling | Created Ticket model with relationships | 3 hours | Task.11 |
| Sept 13 | Database modeling | Created SavedEvent model for calendar | 2 hours | Task.13 |
| Sept 14 | Code review | Reviewed User and Event models | 1 hour | - |

**Total Time This Week:** 11.5 hours

---

## Week 2 (Sept 16 - Sept 22, 2025)

### Activities

| Date | Activity | Description | Time Spent | Related Issue/Task |
|------|----------|-------------|------------|-------------------|
| Sept 16 | Team meeting | Mid-sprint check-in | 1.5 hours | - |
| Sept 17 | Backend development | Implemented login logic with BCrypt | 5 hours | Task.20 |
| Sept 18 | Backend development | Added password hashing on signup | 3 hours | Task.21 |
| Sept 19 | Testing | Tested authentication with all user roles | 3 hours | Task.25 |
| Sept 20 | Backend development | Implemented signup logic for students | 4 hours | Task.21 |
| Sept 21 | Bug fixing | Fixed BCrypt work factor configuration | 2 hours | Task.20 |

**Total Time This Week:** 18.5 hours

---

## Week 3 (Sept 23 - Sept 29, 2025)

### Activities

| Date | Activity | Description | Time Spent | Related Issue/Task |
|------|----------|-------------|------------|-------------------|
| Sept 24 | Team meeting | Sprint 1 review and retrospective | 2 hours | - |
| Sept 25 | Backend development | Created query for approved events | 3 hours | Task.33 |
| Sept 26 | Testing | Tested event browsing functionality | 2 hours | Task.33 |
| Sept 27 | Backend development | Implemented event creation logic | 4 hours | Task.42 |
| Sept 28 | Testing | End-to-end testing of event creation | 2.5 hours | Task.45 |
| Sept 29 | Documentation | Added XML comments to Page Models | 1.5 hours | - |

**Total Time This Week:** 15 hours

---

## Summary

### Key Contributions
- Designed Ticket and SavedEvent entity models
- Implemented complete authentication system with BCrypt password hashing
- Built login and signup logic for all user types
- Created event query services for student browsing
- Implemented event creation backend for organizers

### Tasks Completed
- Task.11 - Create Ticket model
- Task.13 - Create SavedEvent model
- Task.20 - Implement login logic
- Task.21 - Implement signup logic
- Task.33 - Query approved events from database
- Task.42 - Implement event creation logic
- Task.45 - Test event creation workflow

### Challenges Faced
- **Challenge:** BCrypt performance issues on initial implementation
  - **Resolution:** Adjusted work factor to 10 for balanced security and performance
- **Challenge:** Session management not persisting after redirect
  - **Resolution:** Added proper session configuration in Program.cs middleware
- **Challenge:** Event query returning too much data
  - **Resolution:** Implemented filtering and pagination patterns

### Learnings
- Mastered BCrypt.Net library for secure password hashing
- Learned ASP.NET Core session management
- Improved understanding of Razor Pages PageModel architecture
- Enhanced skills in Entity Framework Core LINQ queries
- Learned authentication best practices (password policies, session timeout)

---

**Total Time Spent on Sprint 1:** 45 hours
