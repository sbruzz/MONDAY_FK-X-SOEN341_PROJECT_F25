# Sprint 3 Meeting #01 - Sprint Planning

**Date:** 2025-10-28
**Time:** [Duration: 1.67 hours]
**Location:** Discord Voice Channel
**Scrum Master:** Kevin Ung

## Attendees
- [x] Salvatore Bruzzese
- [x] Souleymane Camara
- [x] Kevin Ung
- [x] Nand Patel
- [x] Dmitrii Cazacu

## Absent
- None

## Agenda
1. Testing of completed web app
2. Sprint 3 review
3. Final testing and verification

## Discussion Points

### 1. Testing of Completed Web App
- Comprehensive testing of all Sprint 3 features
- Verified admin features functionality
- Tested QR scanner implementation
- Verified UI consistency across pages
- Checked mobile responsiveness

### 2. Sprint 2 Retrospective Summary
- **What went well:**
  - Successfully completed all Sprint 2 goals
  - Ticket system working perfectly
  - Good collaboration between team members
  - Organizer dashboard looks professional
- **What could improve:**
  - Need better documentation of APIs
  - Should start testing earlier in sprint
  - Code reviews could be faster

### 2. Sprint 3 Goals
**Sprint Goal:** Implement admin features, QR scanner functionality, and UI polish across the application.

**Key Deliverables:**
- US.03: Administrator features (user approval, event moderation, analytics)
- QR Scanner: File upload validation and ticket redemption tracking
- UI Polish: Consistent styling, mobile responsiveness, loading states
- Additional Feature: To be approved by TA

### 3. User Stories Review
Discussed and prioritized user stories:
- **High Priority:**
  - Task.21: Admin user approval (Foundation for admin features)
  - Task.23: Event moderation backend
  - Task.18/19/20: QR scanner implementation
- **Medium Priority:**
  - Task.22: Organizer approval frontend
  - Task.24: Event moderation frontend
  - Task.25/26: Global analytics dashboard
  - Task.27/28: Organization management
- **Lower Priority:**
  - Task.34/35: Architecture documentation

### 4. Task Assignments

**Backend Team:**
- **Salvatore:** Task.18 (QR scanner integration), Task.19 (QR backend implementation)
- **Dmitrii:** Task.25 (Global stats query), Task.27 (Organizer management backend)
- **Kevin:** Task.21 (Admin approval), Task.23 (Event moderation backend), Task.34 (Backend architecture)

**Frontend Team:**
- **Souleymane:** Task.22 (Organizer approval frontend), Task.26 (Display global stats), Task.28 (Admin management page), Task.35 (Frontend architecture)
- **Nand:** Task.20 (QR upload frontend), Task.24 (Event moderation frontend)

### 5. Technical Decisions

**QR Scanner Approach:**
- Use file upload for QR code images
- Backend will decode and validate QR codes
- Store redemption status in database
- Show recent check-ins on scanner page

**Admin Features:**
- User approval workflow: Pending → Approved/Rejected
- Event moderation: Pending → Approved/Rejected
- Global analytics with real-time statistics
- Organization management with CRUD operations

**UI Polish Strategy:**
- Audit all pages for consistent styling
- Ensure mobile responsiveness
- Add loading states to all forms
- Implement consistent error handling

**Additional Feature Discussion:**
- Team discussed potential additional features
- Options: Carpool system, event recommendations, social features
- Will propose to TA and get approval
- **Decision:** Will decide by end of week 1

### 6. Sprint 3 Timeline (4 Weeks)

**Week 1 (Oct 21-27):**
- Admin approval system
- QR scanner backend
- Start UI audit

**Week 2 (Oct 28-Nov 3):**
- Event moderation
- Global analytics
- QR scanner frontend

**Week 3 (Nov 4-10):**
- Organization management
- UI polish implementation
- Integration testing

**Week 4 (Nov 9-10):**
- Final testing
- Bug fixes
- Documentation
- Sprint review preparation

### 7. Blockers/Issues

**Potential Blocker 1: QR Code Library**
- **Issue:** Need to verify QR code decoding library works well
- **Resolution:** Salvatore will research and test QR decoding libraries this week

**Potential Blocker 2: Admin Analytics Complexity**
- **Issue:** Global analytics might be complex to implement
- **Resolution:** Dmitrii will break down into smaller tasks, start with basic stats

**Potential Blocker 3: Additional Feature Approval**
- **Issue:** Need TA approval for additional feature
- **Resolution:** Kevin will draft proposal and submit by Oct 25

## Action Items
- [ ] Research QR code decoding libraries - Salvatore - Due: Oct 23
- [ ] Create admin approval workflow design - Kevin - Due: Oct 23
- [ ] Draft additional feature proposal - Kevin - Due: Oct 25
- [ ] Break down analytics tasks - Dmitrii - Due: Oct 23
- [ ] Start UI audit - Souleymane, Nand - Due: Oct 24
- [ ] Review all Sprint 3 tasks - All - Due: Oct 22

## Next Meeting
- **Date:** 2025-10-30
- **Time:** 14:00
- **Agenda:** Mid-sprint check-in

## Notes
- Sprint 3 is 4 weeks (longer than previous sprints)
- Team is confident about completing all goals
- Will maintain same velocity as Sprint 2
- Need to balance feature development with UI polish
- Team agreed to daily async standups continue
- Excited to complete the final sprint!

---

**Meeting Adjourned:** 16:00

