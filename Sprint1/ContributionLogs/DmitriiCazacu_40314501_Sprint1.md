# Sprint 1 Contribution Log
**Name:** Dmitrii Cazacu
**Student ID:** 40314501
**GitHub Username:** Hildthelsta
**Role:** Backend Developer

## Week 1 (Sept 9 - Sept 15, 2025)

### Activities

| Date | Activity | Description | Time Spent | Related Issue/Task |
|------|----------|-------------|------------|-------------------|
| Sept 9 | Team meeting | Sprint 1 kickoff and planning | 2 hours | - |
| Sept 10 | Environment setup | Set up development environment, installed .NET SDK | 2.5 hours | - |
| Sept 11 | Repository setup | Cloned repository, reviewed codebase structure | 1.5 hours | Task.03 |
| Sept 12 | Database modeling | Created Organization model | 2 hours | Task.12 |
| Sept 13 | Database configuration | Configured AppDbContext with DbSets | 3 hours | Task.14 |
| Sept 14 | Migration | Created initial database migration | 2 hours | Task.16 |

**Total Time This Week:** 13 hours

---

## Week 2 (Sept 16 - Sept 22, 2025)

### Activities

| Date | Activity | Description | Time Spent | Related Issue/Task |
|------|----------|-------------|------------|-------------------|
| Sept 16 | Team meeting | Mid-sprint check-in | 1.5 hours | - |
| Sept 17 | Backend development | Implemented signup logic for students | 4 hours | Task.21 |
| Sept 18 | Testing | Tested database creation and seed data | 3 hours | Task.17 |
| Sept 19 | Backend development | Created EventDetailsModel PageModel | 3.5 hours | Task.36 |
| Sept 20 | Database optimization | Added indexes to frequently queried columns | 2.5 hours | Task.14 |
| Sept 21 | Code review | Reviewed login backend implementation | 1 hour | Task.20 |

**Total Time This Week:** 15.5 hours

---

## Week 3 (Sept 23 - Sept 29, 2025)

### Activities

| Date | Activity | Description | Time Spent | Related Issue/Task |
|------|----------|-------------|------------|-------------------|
| Sept 24 | Team meeting | Sprint 1 review and retrospective | 2 hours | - |
| Sept 25 | Backend development | Implemented event creation backend logic | 4 hours | Task.42 |
| Sept 26 | Testing | Integration testing for authentication | 2.5 hours | Task.25 |
| Sept 27 | Database work | Added relationship configurations in OnModelCreating | 3 hours | Task.14 |
| Sept 28 | Bug fixing | Fixed foreign key constraint issues | 2 hours | - |
| Sept 29 | Documentation | Documented database schema and relationships | 1.5 hours | - |

**Total Time This Week:** 15 hours

---

## Summary

### Key Contributions
- Created Organization entity model with proper relationships
- Configured AppDbContext with all DbSets and relationships
- Implemented database migrations for initial schema
- Added database indexes for query optimization
- Built event details page backend
- Implemented event creation logic with validation

### Tasks Completed
- Task.03 - Repository setup and review
- Task.12 - Create Organization model
- Task.14 - Configure AppDbContext with DbSets
- Task.16 - Create initial migration
- Task.17 - Test database creation and seeding
- Task.21 - Implement signup logic (student)
- Task.36 - Create EventDetails PageModel
- Task.42 - Implement event creation logic

### Challenges Faced
- **Challenge:** Entity Framework Core foreign key configuration causing circular dependencies
  - **Resolution:** Studied EF Core relationship patterns and implemented proper navigation properties
- **Challenge:** Migration conflicts when multiple developers created migrations simultaneously
  - **Resolution:** Established team workflow for migration creation (one at a time)
- **Challenge:** Database indexes not improving query performance as expected
  - **Resolution:** Analyzed query plans and added composite indexes where needed

### Learnings
- Mastered Entity Framework Core fluent API for relationship configuration
- Learned database migration management and conflict resolution
- Improved understanding of SQL indexing strategies
- Enhanced skills in Razor Pages PageModel architecture
- Learned ASP.NET Core dependency injection patterns

---

**Total Time Spent on Sprint 1:** 43.5 hours
