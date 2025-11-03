# Sprint 2 Contribution Log
**Name:** Dmitrii Cazacu
**Student ID:** 40314501
**GitHub Username:** Hildthelsta
**Role:** Backend Developer

## Week 1 (Sept 30 - Oct 6, 2025)

### Activities

| Date | Activity | Description | Time Spent | Related Issue/Task |
|------|----------|-------------|------------|-------------------|
| Oct 1 | Team meeting | Sprint 2 planning and task assignment | 2 hours | - |
| Oct 2 | Database analysis | Reviewed ticket table schema and relationships | 1.5 hours | Task.08 |
| Oct 3 | Backend development | Implemented event query service for organizers | 4 hours | Task.14 |
| Oct 4 | LINQ optimization | Optimized event queries with Include() for eager loading | 3 hours | Task.14 |
| Oct 5 | Testing | Tested event queries with various filters | 2 hours | Task.14 |

**Total Time This Week:** 12.5 hours

---

## Week 2 (Oct 7 - Oct 13, 2025)

### Activities

| Date | Activity | Description | Time Spent | Related Issue/Task |
|------|----------|-------------|------------|-------------------|
| Oct 8 | Team meeting | Mid-sprint check-in | 1.5 hours | - |
| Oct 9 | Service development | Created DbCSVCommunicator service class | 3 hours | Task.16 |
| Oct 10 | CSV logic | Implemented CSV generation from ticket queries | 5 hours | Task.16 |
| Oct 11 | CSV formatting | Added UTF-8 BOM for Excel compatibility | 2 hours | Task.16 |
| Oct 12 | Testing | Tested CSV export with various data sets | 2.5 hours | Task.16 |
| Oct 13 | Bug fixing | Fixed CSV column alignment issues | 2 hours | Task.16 |

**Total Time This Week:** 16 hours

---

## Week 3 (Oct 14 - Oct 20, 2025)

### Activities

| Date | Activity | Description | Time Spent | Related Issue/Task |
|------|----------|-------------|------------|-------------------|
| Oct 15 | Team meeting | Sprint 2 review and retrospective | 2 hours | - |
| Oct 16 | Code review | Reviewed login backend implementation | 1.5 hours | Task.30 |
| Oct 17 | Bug fixing | Fixed CSV date/time formatting showing ##### in Excel | 3 hours | Task.16 |
| Oct 18 | Testing | Integration testing for complete organizer workflow | 2.5 hours | - |
| Oct 19 | Documentation | Documented CSV export API and usage | 1.5 hours | - |
| Oct 20 | Sprint prep | Prepared Sprint 3 backend tasks | 1.5 hours | - |

**Total Time This Week:** 12 hours

---

## Summary

### Key Contributions
- Implemented efficient LINQ queries for event data retrieval
- Created DbCSVCommunicator service for attendee list export
- Implemented CSV generation with proper UTF-8 encoding for Excel
- Fixed date/time formatting issues in CSV exports
- Optimized database queries using eager loading to prevent N+1 problems

### Tasks Completed
- Task.14 - [Backend] Event query
- Task.16 - [Backend] Query to CSV

### Challenges Faced
- **Challenge:** CSV files showing ##### symbols in Excel for date columns
  - **Resolution:** Split DateTime into separate Date and Time columns with shorter format strings
- **Challenge:** Special characters in CSV breaking formatting
  - **Resolution:** Implemented proper CSV escaping with quotes around text fields
- **Challenge:** Large event attendee lists causing memory issues
  - **Resolution:** Implemented streaming CSV generation for scalability

### Learnings
- Mastered Entity Framework Core query optimization techniques
- Learned CSV file format specifications and Excel compatibility requirements
- Improved understanding of character encoding (UTF-8 BOM)
- Enhanced skills in LINQ to Entities for complex queries

---

**Total Time Spent on Sprint 2:** 40.5 hours
