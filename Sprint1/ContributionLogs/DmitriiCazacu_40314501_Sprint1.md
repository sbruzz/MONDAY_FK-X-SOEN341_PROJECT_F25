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
| Sept 10 | Environment setup | Installed .NET 9.0 SDK and VS Code extensions | 2 hours | - |
| Sept 11 | Repository setup | Cloned repo and reviewed project structure | 1.5 hours | Task.03 |
| Sept 12 | Database modeling | Created Organization model with relationships | 3.5 hours | Task.12 |
| Sept 13 | Code review | Reviewed User and Event models for consistency | 1.5 hours | - |
| Sept 14 | Migration preparation | Prepared database schema for initial migration | 2 hours | Task.15 |

**Total Time This Week:** 12.5 hours

---

## Week 2 (Sept 16 - Sept 22, 2025)

### Activities

| Date | Activity | Description | Time Spent | Related Issue/Task |
|------|----------|-------------|------------|-------------------|
| Sept 16 | Team meeting | Mid-sprint check-in | 1.5 hours | - |
| Sept 17 | Database migration | Created initial EF Core migration | 3 hours | Task.15 |
| Sept 18 | Migration testing | Tested migration and verified table creation | 2.5 hours | Task.17 |
| Sept 19 | Backend development | Added Organization-User relationship logic | 3.5 hours | Task.12 |
| Sept 20 | Testing | Tested database relationships and constraints | 2.5 hours | Task.17 |
| Sept 21 | Bug fixing | Fixed foreign key constraint issues | 2 hours | Task.15 |

**Total Time This Week:** 16 hours

---

## Week 3 (Sept 23 - Sept 29, 2025)

### Activities

| Date | Activity | Description | Time Spent | Related Issue/Task |
|------|----------|-------------|------------|-------------------|
| Sept 24 | Team meeting | Sprint 1 review and retrospective | 2 hours | - |
| Sept 25 | Code review | Reviewed authentication implementation | 1.5 hours | Task.20 |
| Sept 26 | Backend development | Created organization query service | 3 hours | Task.34 |
| Sept 27 | Testing | Tested organization creation and retrieval | 2 hours | Task.34 |
| Sept 28 | Documentation | Added XML comments to Organization model | 1.5 hours | - |
| Sept 29 | Migration update | Updated migration after schema changes | 2 hours | Task.15 |

**Total Time This Week:** 12 hours

---

## Summary

### Key Contributions
- Designed and implemented Organization entity model
- Created Entity Framework Core database migrations
- Established Organization-User relationship in database schema
- Implemented organization query service for data retrieval
- Tested and verified database schema and relationships

### Tasks Completed
- Task.12 - Create Organization model
- Task.15 - Database migration creation and management
- Task.17 - Test database creation and migrations
- Task.34 - Organization query service

### Challenges Faced
- **Challenge:** Migration conflicts when multiple team members worked on models simultaneously
  - **Resolution:** Implemented branch-based migration strategy and coordinated model changes
- **Challenge:** Foreign key constraints causing deletion issues
  - **Resolution:** Configured proper cascade delete behaviors in EF Core
- **Challenge:** Database schema changes requiring multiple migration iterations
  - **Resolution:** Documented migration workflow and created migration rollback procedures

### Learnings
- Mastered Entity Framework Core migration commands and workflows
- Learned database relationship configuration in EF Core (one-to-many, many-to-one)
- Improved understanding of database schema design principles
- Enhanced skills in SQLite database management through EF Core
- Learned migration strategy for team collaboration

---

**Total Time Spent on Sprint 1:** 40.5 hours

