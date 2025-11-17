# Sprint 3 Contribution Log

**Name:** Salvatore Bruzzese

**Student ID:** 40112201

**GitHub Username:** sbruzz

**Role:** Backend Developer

## Week 1 (Oct 21 - Oct 27, 2025)

### Activities

| Date | Activity | Description | Time Spent | Related Issue/Task |
|------|----------|-------------|------------|-------------------|
| Oct 21 | Team meeting | Sprint 3 planning and task assignment | 2.5 hours | - |
| Oct 22 | Backend development | Implemented QR code validation logic | 4 hours | Task.19 |
| Oct 23 | Backend development | Created ticket redemption tracking system | 5 hours | Task.19 |
| Oct 24 | Backend development | Implemented file upload handler for QR images | 4 hours | Task.18 |
| Oct 25 | Testing | Tested QR code validation with various formats | 2.5 hours | Task.19 |
| Oct 26 | Bug fixing | Fixed QR code decoding issues | 2 hours | Task.19 |

**Total Time This Week:** 20 hours

---

## Week 2 (Oct 28 - Nov 3, 2025)

### Activities

| Date | Activity | Description | Time Spent | Related Issue/Task |
|------|----------|-------------|------------|-------------------|
| Oct 30 | Team meeting | Mid-sprint check-in | 1.5 hours | - |
| Nov 1 | Backend development | Enhanced QR scanner with duplicate detection | 3.5 hours | Task.19 |
| Nov 2 | Backend development | Implemented ticket redemption status updates | 3 hours | Task.19 |
| Nov 3 | Testing | Comprehensive testing of QR scanner workflow | 3 hours | Task.19 |
| Nov 3 | Code review | Reviewed admin backend implementations | 1.5 hours | Task.21, Task.23 |

**Total Time This Week:** 12.5 hours

---

## Week 3 (Nov 4 - Nov 8, 2025)

### Activities

| Date | Activity | Description | Time Spent | Related Issue/Task |
|------|----------|-------------|------------|-------------------|
| Nov 4 | Backend development | Added QR scanner statistics to organizer dashboard | 3 hours | Task.18 |
| Nov 5 | Testing | Integration testing of QR scanner with frontend | 3 hours | Task.18 |
| Nov 6 | Bug fixing | Fixed QR code validation edge cases | 2.5 hours | Task.19 |
| Nov 7 | Documentation | Documented QR scanner API and usage | 2 hours | Task.18 |
| Nov 8 | Code review | Reviewed frontend QR scanner implementation | 1.5 hours | Task.20 |

**Total Time This Week:** 12 hours

---

## Week 3 Continued (Nov 9 - Nov 10, 2025)

### Activities

| Date | Activity | Description | Time Spent | Related Issue/Task |
|------|----------|-------------|------------|-------------------|
| Nov 9 | Team meeting | Sprint 3 review and retrospective | 2.5 hours | - |
| Nov 9 | Testing | End-to-end testing of complete QR scanner feature | 3 hours | Task.18, Task.19 |
| Nov 10 | Performance optimization | Optimized QR code processing for large batches | 2.5 hours | Task.19 |
| Nov 10 | Final testing | Regression testing of QR scanner features | 2 hours | Task.18, Task.19 |

**Total Time This Week:** 10 hours

---

## Summary

### Key Contributions
- Implemented QR code validation and decoding system
- Created ticket redemption tracking with duplicate prevention
- Built file upload handler for QR code image processing
- Integrated QR scanner with organizer dashboard
- Added comprehensive QR scanner statistics and reporting
- Optimized QR code processing for performance

### Tasks Completed
- Task.18 - [Backend] QR scanner integration
- Task.19 - [Backend] QR implementation

### Challenges Faced
- **Challenge:** QR code image decoding from file uploads
  - **Resolution:** Integrated image processing library and implemented proper format validation
- **Challenge:** Preventing duplicate ticket redemptions
  - **Resolution:** Implemented database-level checks with transaction locking
- **Challenge:** QR code validation failing for rotated images
  - **Resolution:** Added image rotation detection and correction logic
- **Challenge:** Performance issues with large QR code batches
  - **Resolution:** Implemented batch processing and caching for QR decoder instances

### Learnings
- Mastered QR code image processing and decoding
- Learned file upload handling in ASP.NET Core
- Improved understanding of image processing libraries
- Enhanced skills in transaction management for preventing race conditions
- Learned performance optimization techniques for batch processing

---

**Total Time Spent on Sprint 3:** 54.5 hours

