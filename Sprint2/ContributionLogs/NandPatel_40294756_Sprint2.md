# Sprint 2 Contribution Log
**Name:** Nand Patel
**Student ID:** 40294756
**GitHub Username:** ns-1456
**Role:** Frontend Developer

## Week 1 (Sept 30 - Oct 6, 2025)

### Activities

| Date | Activity | Description | Time Spent | Related Issue/Task |
|------|----------|-------------|------------|-------------------|
| Oct 1 | Team meeting | Sprint 2 planning and task assignment | 2 hours | - |
| Oct 2 | Research | Researched session management in Razor Pages | 2 hours | Task.32 |
| Oct 3 | Frontend development | Started session-based home page routing | 3 hours | Task.32 |
| Oct 4 | Page models | Implemented role-based redirects in PageModels | 4 hours | Task.32 |
| Oct 5 | Testing | Tested session handling for different roles | 2 hours | Task.32 |

**Total Time This Week:** 13 hours

---

## Week 2 (Oct 7 - Oct 13, 2025)

### Activities

| Date | Activity | Description | Time Spent | Related Issue/Task |
|------|----------|-------------|------------|-------------------|
| Oct 8 | Team meeting | Mid-sprint check-in | 1.5 hours | - |
| Oct 9 | Frontend development | Created role-specific layouts | 5 hours | Task.32 |
| Oct 10 | Session handling | Implemented session checks on all protected pages | 4 hours | Task.32 |
| Oct 11 | Bug fixing | Fixed session timeout redirect issues | 2.5 hours | Task.32 |
| Oct 12 | Testing | Tested session persistence across pages | 2 hours | Task.32 |
| Oct 13 | Code review | Reviewed login backend implementation | 1 hour | Task.30 |

**Total Time This Week:** 16 hours

---

## Week 3 (Oct 14 - Oct 20, 2025)

### Activities

| Date | Activity | Description | Time Spent | Related Issue/Task |
|------|----------|-------------|------------|-------------------|
| Oct 15 | Team meeting | Sprint 2 review and retrospective | 2 hours | - |
| Oct 16 | UI polish | Added loading spinners to forms | 2 hours | - |
| Oct 17 | Frontend development | Improved error message display | 2.5 hours | - |
| Oct 18 | Testing | Comprehensive testing of authentication flows | 3 hours | - |
| Oct 19 | Bug fixing | Fixed edge cases in session management | 2 hours | Task.32 |
| Oct 20 | Documentation | Documented session handling patterns | 1.5 hours | - |

**Total Time This Week:** 13 hours

---

## Summary

### Key Contributions
- Implemented session-based authentication across all pages
- Created role-specific home page routing (Student/Organizer/Admin)
- Built session validation middleware for protected pages
- Implemented automatic redirect on session expiration
- Added loading states and improved error handling

### Tasks Completed
- Task.32 - [Frontend] Session-based home pages

### Challenges Faced
- **Challenge:** Session data not persisting across page reloads in some browsers
  - **Resolution:** Configured session cookies with HttpOnly and SameSite policies
- **Challenge:** Circular redirects when session expired
  - **Resolution:** Added proper return URL handling to avoid redirect loops
- **Challenge:** Role-based routing logic becoming complex
  - **Resolution:** Created helper method in base page model for cleaner code

### Learnings
- Mastered ASP.NET Core session management
- Learned about cookie security best practices (HttpOnly, Secure, SameSite)
- Improved understanding of authentication vs authorization
- Enhanced skills in debugging browser storage issues
- Learned state management patterns in server-side applications

---

**Total Time Spent on Sprint 2:** 42 hours
