# Sprint 2 Meeting #03 - Sprint Review & Retrospective

**Date:** 2025-10-15
**Time:** 14:00 - 16:30
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
1. Sprint 2 Demo (All completed features)
2. Sprint metrics review
3. Retrospective (What went well / What to improve)
4. Sprint 3 preparation

---

## Part 1: Sprint Review (14:00 - 15:15)

### Sprint 2 Goal Recap
**Goal:** Implement core ticketing system with QR codes, organizer analytics, and CSV export functionality

**Result:** ✅ **GOAL ACHIEVED** - All planned features delivered

### Features Demonstrated

#### 1. Ticket System (Salvatore & Jack)
**Demonstrated:**
- Student can claim tickets for events from event details page
- Unique QR code generated automatically for each ticket
- Tickets stored with GUID unique code
- QR code displayed on My Tickets page
- Ticket capacity enforcement working correctly

**Feedback:**
- TA might ask for ticket download feature in future
- Consider adding ticket cancellation/transfer in Sprint 3

#### 2. Login & Session Management (Salvatore, Nand)
**Demonstrated:**
- Improved login page with better UI
- Session-based authentication working
- Role-specific home page routing (Student/Organizer/Admin)
- Automatic redirect when session expires

**Feedback:**
- Very smooth user experience
- Sessions working correctly across all pages

#### 3. Organizer Dashboard (Souleymane & Dmitrii)
**Demonstrated:**
- Beautiful dashboard with metric cards
- Shows total events, tickets sold, attendance rate
- Upcoming events timeline with date badges
- Recent activity feed
- Most popular event highlight

**Feedback:**
- Dashboard looks professional and polished
- Gradient color scheme matches project theme well

#### 4. CSV Export (Dmitrii & Nand)
**Demonstrated:**
- Export button on organizer event details
- CSV generates with all attendee information
- UTF-8 BOM for Excel compatibility
- Separate date and time columns (no more ##### symbols!)

**Feedback:**
- CSV opens perfectly in Excel
- Column headers clear and descriptive

#### 5. Event Creation (Kevin)
**Demonstrated:**
- Full event creation form with validation
- Events linked to organizer who creates them
- Events start in Pending approval status

**Feedback:**
- Form validation working well
- Ready for admin moderation in Sprint 3

#### 6. Home Page Improvements (Souleymane)
**Demonstrated:**
- Redesigned event browsing with improved card layout
- Better responsive design
- Cleaner navigation

**Feedback:**
- Much better than Sprint 1 version
- Cards look professional

### Sprint 2 Metrics

**Story Points:**
- Planned: 47 points
- Completed: 47 points
- **Velocity: 100%** ✅

**Tasks Completed:**
- Task.08, Task.10, Task.11, Task.12, Task.13, Task.14, Task.15, Task.16, Task.17, Task.29, Task.30, Task.31, Task.32 (reassigned from Jack)

**Bugs Fixed:** 5 bugs found and fixed during sprint

**Code Quality:**
- 12 Pull Requests merged
- Average PR review time: ~6 hours
- All code reviewed before merge

---

## Part 2: Sprint Retrospective (15:15 - 16:15)

### What Went Well 🎉

1. **Great Collaboration:**
   - Frontend and backend teams worked well together
   - Quick resolution of API contract issues
   - Helpful code reviews

2. **Technical Achievements:**
   - QRCoder integration was smooth
   - CSV export working perfectly
   - Dashboard looks professional

3. **Process Improvements:**
   - Mid-sprint check-in very helpful
   - Async standups working well
   - GitHub issue tracking clear

4. **Individual Highlights:**
   - Salvatore's ticket system is solid foundation
   - Souleymane's dashboard design is beautiful and took on extra frontend work
   - Dmitrii solved CSV formatting issues creatively
   - Nand's session management implementation and took on additional frontend tasks
   - Kevin's coordination and event creation

### What Could Be Improved 🔧

1. **Testing:**
   - **Issue:** Testing happened mostly at end of sprint
   - **Impact:** Found some bugs late
   - **Action:** Start testing earlier in Sprint 3, test each feature as soon as completed

2. **Documentation:**
   - **Issue:** Backend API changes not always documented
   - **Impact:** Frontend team sometimes confused about data structure
   - **Action:** Add XML comments to all PageModel handlers, create API documentation page

3. **Time Estimation:**
   - **Issue:** Some tasks took longer than estimated (CSV issues, QR performance)
   - **Impact:** Had to adjust plan mid-sprint
   - **Action:** Add buffer time to complex technical tasks, use past sprint data for better estimates

4. **Code Review Speed:**
   - **Issue:** Some PRs waited >24 hours for review
   - **Impact:** Blocked dependent work
   - **Action:** Set up PR review assignments, aim for <12 hour review time

### Action Items for Sprint 3

**Process Changes:**
- [ ] **Implement test-driven development approach** - All devs - Sprint 3
- [ ] **Create API documentation page** - Backend team - Week 1 of Sprint 3
- [ ] **Set up automated PR review reminders** - Kevin - Oct 16
- [ ] **Add estimation buffer (1.5x) for technical unknowns** - Team - Sprint 3 planning

**Technical Debt:**
- [ ] **Add unit tests for ticket claiming logic** - Salvatore - Sprint 3
- [ ] **Refactor repeated queries into repository pattern** - Dmitrii - Sprint 3 (if time)
- [ ] **Add error logging system** - TBD - Sprint 3 or 4

### Team Sentiment Check

**How do you feel about the sprint? (1-5 scale, 5 = excellent)**

- Salvatore: 4/5 - "Proud of ticket system, wish we started testing earlier"
- Souleymane: 5/5 - "Dashboard turned out great, good collaboration"
- Dmitrii: 4/5 - "CSV issues were frustrating but solved, learned a lot"
- Jack: 4/5 - "Happy with output, code reviews could be faster"
- Kevin: 4/5 - "Good sprint, need better estimation next time"
- Nand: 4/5 - "Session management was tricky but works well now"

**Average: 4.2/5** - Strong sprint overall!

---

## Part 3: Sprint 3 Preview (16:15 - 16:30)

### Sprint 3 Focus Areas (Oct 21 - Nov 17, 4 weeks)

**Main Goals:**
1. **Admin Features:**
   - User account approval
   - Event moderation
   - Global analytics dashboard
   - Organization management

2. **QR Scanner:**
   - File upload for QR validation
   - Ticket redemption tracking

3. **UI Polish:**
   - Consistent styling across all pages
   - Mobile responsiveness
   - Loading states and animations

4. **Additional Feature:**
   - Team will propose and implement one complex feature
   - To be approved by TA

### Preparation Tasks
- Kevin will create Sprint 3 issues by Oct 17
- Team to review and estimate Sprint 3 backlog by Oct 19
- Sprint 3 planning meeting: Oct 21 at 14:00

### Key Dates
- Sprint 3 Start: Oct 21
- Mid-sprint check-in: Oct 30
- Sprint 3 End: Nov 17
- Final project due: Nov 20

## Notes
- Excellent sprint! Team delivered everything planned
- Team velocity consistent at 45-47 points per sprint
- Sprint 3 will be 4 weeks (longer for admin features + additional feature)
- All team members confirmed availability for Sprint 3
- Confidence is high going into final sprint

---

**Meeting Adjourned:** 16:30
**Next Meeting:** Sprint 3 Planning - Oct 21, 2025 at 14:00
