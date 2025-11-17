# Sprint 1 Meeting #02 - Mid-Sprint Check-In

**Date:** 2025-09-16
**Time:** 14:00 - 15:30
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
1. Sprint progress review
2. Demo completed work
3. Discuss blockers
4. Plan remaining tasks

## Discussion Points

### 1. Progress Update
**Note:** This meeting focused on adding info to Moodle team sheet and adding everyone to Git.

**Burndown Status:**
- Started with ~35 story points
- Completed ~15 story points (43% done)
- On track to complete sprint goal

**Velocity Note:** Team is making good progress, slightly ahead of schedule

### 2. Completed Tasks (Week 1)

- **Task.01** - Event database schema setup - Kevin ✅
  - Demo: Showed Event model with all required fields
  - Created initial migration

- **Task.02** - Project initialization - Kevin ✅
  - Demo: ASP.NET Core 9.0 Razor Pages project created
  - Repository structure established

- **Task.08** - ERD design - Kevin ✅
  - Demo: Displayed complete ERD with all entities and relationships
  - Team reviewed and approved design

- **Task.09** - User model creation - Kevin ✅
  - Demo: User model with Role enum and all required fields

- **Task.11** - Ticket model creation - Salvatore ✅
  - Demo: Ticket model with relationships to User and Event

- **Task.12** - Organization model creation - Dmitrii ✅
  - Demo: Organization model with proper relationships

- **Task.13** - SavedEvent model creation - Salvatore ✅
  - Demo: SavedEvent model for calendar feature

- **Task.15** - EF Core setup - Kevin ✅
  - Demo: SQLite database connection configured
  - AppDbContext created

- **Task.18** - Login page creation - Souleymane ✅
  - Demo: Basic login page with form structure

- **Task.24** - Home page creation - Souleymane ✅
  - Demo: Home page layout with event listings

### 3. In-Progress Tasks (Week 2)

- **Task.10** - Event model finalization - Kevin (90% complete)
  - Adding approval status field

- **Task.20** - Login logic implementation - Salvatore (70% complete)
  - BCrypt integration done, working on session management

- **Task.21** - Signup logic implementation - Salvatore (60% complete)
  - Student signup working, starting organizer signup

- **Task.22** - Session configuration - Kevin (80% complete)
  - Session middleware configured, testing persistence

- **Task.26** - Events browsing page - Souleymane (50% complete)
  - Layout done, integrating with backend queries

- **Task.27** - Event details page - Souleymane (40% complete)
  - Starting implementation

- **Task.19** - Signup pages - Nand (60% complete)
  - Student signup page done, working on organizer signup

- **Task.04** - Shared layouts - Nand (70% complete)
  - Base layout done, working on role-specific layouts

### 4. Blockers/Issues Discussed

**Blocker 1: BCrypt Configuration**
- **Issue:** Salvatore having issues with BCrypt work factor
- **Discussion:** Performance vs security trade-off
- **Resolution:** Set work factor to 10 for balanced performance

**Blocker 2: Database Migration Conflicts**
- **Issue:** Multiple team members creating migrations simultaneously
- **Discussion:** Need better coordination on model changes
- **Resolution:** Designate Kevin as migration coordinator, review changes before migration

**Blocker 3: Session Management**
- **Issue:** Session not persisting after redirect
- **Discussion:** Session middleware order in pipeline
- **Resolution:** Kevin to review Program.cs middleware order

**Blocker 4: Frontend-Backend Integration**
- **Issue:** Frontend pages need data from backend, but queries not ready
- **Discussion:** Need mock data or parallel development
- **Resolution:** Frontend will use placeholder data, backend will provide queries by Sept 20

### 5. Code Review Notes

- **Good:** Kevin's ERD is well-designed and comprehensive
- **Good:** Salvatore's models have proper relationships
- **Improvement needed:** Need more comments in code
- **Improvement needed:** Some models missing validation attributes

**Action:** Team agreed to add validation attributes to all models

### 6. Plan for Remaining Sprint

**This week priorities:**
1. Complete authentication system (Salvatore, Kevin)
2. Finish signup pages (Nand, Salvatore)
3. Complete frontend pages (Souleymane)
4. Finalize database migrations (Dmitrii, Kevin)
5. Integration testing

**Next week (final week):**
- End-to-end testing
- Bug fixes
- UI polish
- Documentation
- Sprint 1 review preparation

## Action Items
- [x] Fix BCrypt work factor configuration - Salvatore - Due: Sept 17
- [x] Review and merge database migrations - Kevin - Due: Sept 18
- [x] Fix session persistence issue - Kevin - Due: Sept 17
- [x] Add validation attributes to models - All - Due: Sept 18
- [x] Create event query service for frontend - Salvatore - Due: Sept 20
- [x] Complete signup pages - Nand - Due: Sept 19
- [x] Complete events browsing page - Souleymane - Due: Sept 20

## Next Meeting
- **Date:** 2025-09-24
- **Time:** 14:00
- **Agenda:** Sprint 1 review and retrospective

## Notes
- Team is working well together
- Good progress on foundation tasks
- Need to speed up integration between frontend and backend
- Discussed need for better communication on API contracts
- Will schedule extra pairing session for authentication integration
- Team morale is high, everyone learning a lot

---

**Meeting Adjourned:** 15:30


