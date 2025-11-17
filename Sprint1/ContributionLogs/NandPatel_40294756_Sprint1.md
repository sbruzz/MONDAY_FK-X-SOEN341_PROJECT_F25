# Sprint 1 Contribution Log
**Name:** Nand Patel
**Student ID:** 40294756
**GitHub Username:** ns-1456
**Role:** Frontend Developer

## Week 1 (Sept 9 - Sept 15, 2025)

### Activities

| Date | Activity | Description | Time Spent | Related Issue/Task |
|------|----------|-------------|------------|-------------------|
| Sept 9 | Team meeting | Sprint 1 kickoff and planning | 2 hours | - |
| Sept 10 | Environment setup | Installed .NET 9.0 SDK and development tools | 2 hours | - |
| Sept 11 | Repository setup | Cloned repo and set up local development | 1.5 hours | Task.03 |
| Sept 12 | Research | Studied Razor Pages and ASP.NET Core architecture | 3 hours | - |
| Sept 13 | Frontend development | Created SignupStudent.cshtml page | 3.5 hours | Task.19 |
| Sept 14 | CSS styling | Added styling to signup form | 2 hours | Task.29 |

**Total Time This Week:** 14 hours

---

## Week 2 (Sept 16 - Sept 22, 2025)

### Activities

| Date | Activity | Description | Time Spent | Related Issue/Task |
|------|----------|-------------|------------|-------------------|
| Sept 16 | Team meeting | Mid-sprint check-in | 1.5 hours | - |
| Sept 17 | Form validation | Added client-side and server-side validation | 4 hours | Task.24 |
| Sept 18 | Frontend development | Created SignupOrganizer.cshtml page | 3.5 hours | Task.39 |
| Sept 19 | Form handling | Integrated signup forms with backend | 3 hours | Task.19, Task.39 |
| Sept 20 | CSS styling | Styled signup pages with brand colors | 2.5 hours | Task.29 |
| Sept 21 | Testing | Tested signup flows for all user types | 2 hours | Task.24 |

**Total Time This Week:** 16.5 hours

---

## Week 3 (Sept 23 - Sept 29, 2025)

### Activities

| Date | Activity | Description | Time Spent | Related Issue/Task |
|------|----------|-------------|------------|-------------------|
| Sept 24 | Team meeting | Sprint 1 review and retrospective | 2 hours | - |
| Sept 25 | Form enhancement | Added organization field to organizer signup | 2.5 hours | Task.39 |
| Sept 26 | Frontend development | Created search and filter components | 3.5 hours | Task.37 |
| Sept 27 | Testing | Tested form validation edge cases | 2 hours | Task.24 |
| Sept 28 | Bug fixing | Fixed validation issues on signup forms | 2 hours | Task.19 |
| Sept 29 | Documentation | Documented form validation patterns | 1.5 hours | - |

**Total Time This Week:** 13.5 hours

---

## Summary

### Key Contributions
- Created student signup page with full form validation
- Implemented organizer signup page with organization selection
- Added comprehensive form validation (client-side and server-side)
- Built search and filter components for event browsing
- Styled all forms with consistent branding
- Implemented Razor validation tag helpers

### Tasks Completed
- Task.03 - Repository setup
- Task.19 - Create SignupStudent page
- Task.24 - Add form validation (client & server)
- Task.29 - Add CSS styling
- Task.37 - Add navigation between pages
- Task.39 - Add organization field to signup

### Challenges Faced
- **Challenge:** Form validation not displaying errors properly
  - **Resolution:** Used Razor validation tag helpers and jQuery validation for proper error display
- **Challenge:** Signup form redirecting before validation complete
  - **Resolution:** Added preventDefault() on form submit and proper async handling
- **Challenge:** Password confirmation validation not working
  - **Resolution:** Implemented custom validation attribute [Compare] for matching passwords
- **Challenge:** Dropdown for organizations not populating
  - **Resolution:** Fixed PageModel to load organizations in OnGetAsync method

### Learnings
- Mastered ASP.NET Core form handling and model binding
- Learned Razor validation tag helpers and data annotations
- Improved understanding of client-side vs server-side validation
- Enhanced skills in jQuery validation library
- Learned custom validation attributes in C#
- Gained experience in responsive form design

---

**Total Time Spent on Sprint 1:** 44 hours
