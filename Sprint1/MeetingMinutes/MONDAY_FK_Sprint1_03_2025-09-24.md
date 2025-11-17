# Sprint 1 Meeting #03 - Sprint Review & Retrospective

**Date:** 2025-09-24
**Time:** 14:00 - 16:00
**Location:** Discord Voice Channel
**Scrum Master:** Kevin Ung

## Attendees
- [x] Salvatore Bruzzese
- [x] Souleymane Camara
- [x] Dmitrii Cazacu
- [x] Kevin Ung
- [x] Nand Patel

## Absent
- Sana Asgharpour
- Jack Di Spirito

## Agenda
1. Sprint 1 review (demonstrate completed features)
2. Sprint metrics and velocity
3. Sprint retrospective (what went well, what to improve)
4. Sprint 2 planning preview

## Sprint Review

### Sprint Goal Achievement
**Sprint Goal:** Establish project foundation with database schema, authentication system, and basic user interface.

**Status:** ✅ **ACHIEVED** - All core deliverables completed successfully

### Completed User Stories

**US.01: Student Event Experience (Foundation)**
- [x] Browse events page with event listings
- [x] Event details page
- [x] Login/Signup for students
- [x] Session authentication

**US.02: Organizer Event Management (Foundation)**
- [x] Organizer signup page
- [x] Event creation backend logic
- [x] Organization model and relationships

**US.03: Administrator Dashboard (Foundation)**
- [x] Database schema with role-based access
- [x] User approval workflow structure

### Feature Demonstrations

**1. Database Schema & Models (Kevin, Salvatore, Dmitrii)**
- **Demo:** Complete ERD with 5 main entities (User, Event, Ticket, Organization, SavedEvent)
- **Demo:** Database migration created and applied successfully
- **Demo:** SQLite database with proper foreign key relationships
- **Feedback:** Team impressed with schema design, comprehensive relationships
- **Status:** Production-ready

**2. Authentication System (Salvatore, Kevin)**
- **Demo:** Student login with BCrypt password verification
- **Demo:** Student and organizer signup with password hashing
- **Demo:** Session management with 20-minute timeout
- **Demo:** Role-based access control foundation
- **Feedback:** Secure implementation, proper session handling
- **Status:** Core authentication complete, needs admin features in Sprint 2

**3. Frontend Pages (Souleymane, Nand)**
- **Demo:** Home page with beige/brown theme (#d5bdaf, #c9a999, #f5ebe0)
- **Demo:** Login page with form validation
- **Demo:** Student and organizer signup pages
- **Demo:** Events browsing page with card layout
- **Demo:** Event details page
- **Demo:** Responsive design (mobile and desktop)
- **Feedback:** Clean design, consistent branding, good UX
- **Status:** Core pages complete, needs event creation UI in Sprint 2

**4. Backend Services (Salvatore, Dmitrii)**
- **Demo:** Event query service for approved events
- **Demo:** Event creation logic with validation
- **Demo:** Organization management
- **Demo:** Database indexing for query optimization
- **Feedback:** Well-structured code, proper separation of concerns
- **Status:** Core services ready, needs organizer dashboard in Sprint 2

### Sprint Metrics

**Story Points:**
- Planned: 35 story points
- Completed: 35 story points
- Completion Rate: 100%

**Tasks:**
- Total Tasks: 28
- Completed: 28
- In Progress: 0
- Not Started: 0

**Velocity:**
- Sprint 1 Velocity: 35 story points
- Team Capacity: 5 active members
- Average Points per Member: 7 story points

**Time Tracking:**
- Kevin: 41.5 hours
- Salvatore: 45 hours
- Dmitrii: 43.5 hours
- Souleymane: 46 hours
- Nand: 44 hours
- **Total Team Effort:** 220 hours (3 weeks)

**Quality Metrics:**
- Code Reviews: 15 pull requests reviewed and merged
- Bugs Found: 6 (all resolved)
- Test Coverage: Manual testing completed for all features

## Sprint Retrospective

### What Went Well ✅

**Technical Achievements:**
- Successfully set up ASP.NET Core 9.0 project structure
- Implemented secure authentication with BCrypt
- Created comprehensive database schema on first try
- Frontend and backend integration smooth
- All team members learned new technologies effectively

**Team Collaboration:**
- Good communication in Discord
- Effective task distribution across frontend/backend
- Pair programming sessions helped knowledge sharing
- Code reviews caught issues early
- Team members helped each other learn

**Process:**
- Sprint planning was detailed and helpful
- Mid-sprint check-in identified blockers early
- GitHub workflow worked well
- Regular async standups kept everyone aligned

### What Didn't Go Well / Challenges ❌

**Technical Issues:**
- **BCrypt Configuration:** Initial performance issues with work factor (resolved: set to 10)
- **Database Migrations:** Conflicts when multiple devs created migrations simultaneously
- **Session Management:** Session not persisting after redirect initially
- **Frontend-Backend Integration:** Timing issues with query readiness

**Process Issues:**
- Some team members new to ASP.NET Core had learning curve
- Database migration coordination needed better workflow
- Not enough time for automated testing
- Documentation could be more detailed

**Team Dynamics:**
- Jack did not contribute code despite attending early meetings
- Sana absent throughout sprint (personal reasons)
- Some tasks took longer than estimated

### Action Items for Improvement 🎯

**Process Improvements:**
1. **Database Migrations:** Designate Kevin as migration coordinator, require review before creating migrations
2. **Testing:** Allocate time for unit testing in Sprint 2
3. **Documentation:** Add XML comments to all public methods and classes
4. **Estimation:** Use Fibonacci sequence for story point estimation
5. **Code Standards:** Create a team style guide document

**Technical Improvements:**
1. Add validation attributes to all models
2. Implement error logging and monitoring
3. Add automated tests for authentication flows
4. Improve error handling and user feedback messages
5. Set up continuous integration with GitHub Actions

**Team Communication:**
1. Keep async standups more consistent
2. Document API contracts before implementation
3. Schedule pairing sessions for complex features
4. Share learning resources in Discord

### Individual Highlights

**Kevin (Scrum Master + Backend):**
- Excellent leadership and coordination
- Designed comprehensive database schema
- Set up solid project foundation
- Helped team members troubleshoot issues

**Salvatore (Backend):**
- Implemented secure authentication system
- Great work on BCrypt integration
- Built robust backend services
- Strong problem-solving skills

**Dmitrii (Backend):**
- Solid database modeling work
- Added valuable query optimizations
- Good attention to detail on relationships
- Helped with migration management

**Souleymane (Frontend):**
- Created beautiful, responsive UI
- Established consistent design language
- Great work on event browsing experience
- Strong CSS and layout skills

**Nand (Frontend):**
- Excellent form validation implementation
- Comprehensive signup flows
- Good attention to UX details
- Solid integration with backend

### Team Morale
- Overall: **High** 😊
- Team is excited about Sprint 1 accomplishments
- Feeling confident about Sprint 2 goals
- Good energy and collaboration
- Everyone learned a lot and feels productive

## Risks & Blockers for Sprint 2

**Risk 1: Team Capacity**
- **Risk:** Sana still unavailable, Jack not contributing
- **Impact:** 5 effective members instead of 7
- **Mitigation:** Adjust sprint capacity accordingly, consider backlog prioritization

**Risk 2: Technical Complexity**
- **Risk:** Sprint 2 includes QR code generation and CSV export
- **Impact:** May require more research and development time
- **Mitigation:** Spike tasks for QR codes, research QRCoder library early

**Risk 3: Integration Testing**
- **Risk:** No automated tests yet
- **Impact:** Regression risks as features grow
- **Mitigation:** Start with critical path tests, expand coverage incrementally

## Sprint 2 Preview

### Sprint 2 Goals (Sept 30 - Oct 20, 2025)
1. Implement ticket claiming and QR code generation
2. Build organizer event creation UI
3. Create organizer dashboard with analytics
4. Implement admin approval workflows
5. Add CSV export functionality

### Priority User Stories
- US.01: Complete student ticket claiming with QR codes
- US.02: Organizer event management dashboard
- US.03: Admin user approval and event moderation

### Estimated Story Points: 40 points
(Slight increase from Sprint 1 based on velocity)

## Action Items

**Before Sprint 2:**
- [ ] Kevin: Create Sprint 2 backlog and task breakdown - Due: Sept 26
- [ ] All: Review Sprint 2 user stories - Due: Sept 27
- [ ] Salvatore: Research QRCoder library - Due: Sept 28
- [ ] Dmitrii: Research CSV export patterns - Due: Sept 28
- [ ] Souleymane: Create mockups for organizer dashboard - Due: Sept 29
- [ ] Nand: Create mockups for admin approval UI - Due: Sept 29
- [ ] All: Complete contribution logs and time tracking - Due: Sept 29

**Sprint 2 Preparation:**
- [ ] Kevin: Schedule Sprint 2 planning meeting - Due: Sept 25
- [ ] All: Review technical documentation for QR codes and CSV - Due: Sept 29
- [ ] Kevin: Update GitHub project board for Sprint 2 - Due: Sept 30

## Next Meeting
- **Date:** 2025-09-30
- **Time:** 14:00
- **Agenda:** Sprint 2 planning and kickoff

## Closing Notes

**Team Accomplishments:**
- Delivered all Sprint 1 commitments on time
- Built solid foundation for campus events platform
- Established good team dynamics and workflow
- Everyone contributed meaningfully and learned new skills
- High quality code with proper architecture

**Scrum Master Notes (Kevin):**
- Proud of the team's achievements in Sprint 1
- Good energy and collaboration throughout the sprint
- Team adapted well to new technologies
- Ready to tackle more complex features in Sprint 2
- Will adjust capacity planning for 5 active members going forward

**Team Sentiment:**
- Excited about what we built
- Confident in our ability to deliver Sprint 2
- Appreciative of team support and collaboration
- Looking forward to adding more user-facing features

---

**Meeting Adjourned:** 16:00

**Sprint 1 Status:** ✅ **SUCCESSFULLY COMPLETED**
