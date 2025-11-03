# Sprint 2 Meeting #02 - Mid-Sprint Check-In

**Date:** 2025-10-08
**Time:** 14:00 - 15:30
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
1. Sprint progress review (burndown)
2. Demo completed work
3. Discuss blockers
4. Adjust plan for remaining week
5. Code review status

## Discussion Points

### 1. Sprint Progress (Week 1 Complete)
**Burndown Status:**
- Started with ~48 story points
- Completed ~20 story points (42% done)
- On track to complete sprint goal

**Velocity Note:** Team is slightly ahead of planned velocity. Good progress!

### 2. Completed Tasks (Week 1)
- **Task.08** - Ticket table creation - Salvatore ✅
  - Demo: Showed Ticket entity model and migration
  - All fields present: UniqueCode, QrCodeBase64, ClaimedAt, IsRedeemed
- **Task.10** - Student-ticket relationship - Salvatore ✅
  - Demo: Tested ticket creation with proper foreign keys
- **Task.12** - Organizer-event relationship - Kevin ✅
  - Demo: Events now properly linked to organizers
- **Task.14** - Event query optimization - Dmitrii ✅
  - Demo: Queries now use Include() for efficient loading

### 3. In-Progress Tasks (Week 2)
- **Task.11** - Ticket claiming with QR - Salvatore (80% complete)
  - QRCoder integrated, working on frontend display
- **Task.13** - Event creation form - Kevin (70% complete)
  - Form complete, adding validation
- **Task.15** - Organizer dashboard - Souleymane (60% complete)
  - Layout done, working on data integration
- **Task.16** - CSV generation - Dmitrii (50% complete)
  - Service created, testing encoding
- **Task.29** - Home page redesign - Souleymane (80% complete)
  - Event cards complete, testing responsiveness
- **Task.31** - Login frontend - Nand (40% complete)
  - Starting this week
- **Task.32** - Session-based routing - Nand (30% complete)
  - Researching best approaches

### 4. Blockers/Issues Discussed

**Blocker 1: QR Code Performance**
- **Issue:** Salvatore reported QR generation slow for batch ticket creation
- **Discussion:** Team discussed caching QRCodeGenerator instance
- **Resolution:** Salvatore will implement singleton pattern for QRCodeGenerator

**Blocker 2: CSV Date Formatting**
- **Issue:** Dmitrii found dates showing as ##### in Excel
- **Discussion:** Column width issue vs format issue
- **Resolution:** Will split DateTime into separate Date and Time columns

**Blocker 3: Frontend-Backend API Mismatch**
- **Issue:** Souleymane's dashboard expecting different data structure
- **Discussion:** Need better API documentation
- **Resolution:** Backend team will add XML comments to PageModel handlers

### 5. Code Review Notes
- **Good:** Salvatore's ticket table is well-structured with proper indexes
- **Good:** Kevin's event creation has thorough validation
- **Improvement needed:** Need more unit tests for ticket claiming logic
- **Improvement needed:** Should extract repeated SQL queries into repository pattern

**Action:** Team agreed to do code reviews within 24 hours of PR creation

### 6. Plan for Remaining Sprint
**This week priorities:**
1. Complete ticket claiming end-to-end (Salvatore, Jack)
2. Finish organizer dashboard (Souleymane, Dmitrii)
3. Complete login improvements (Salvatore, Jack, Nand)
4. Start CSV export testing (Dmitrii)

**Next week (final week):**
- Integration testing
- Bug fixes
- Polish and documentation

## Action Items
- [x] Implement QRCodeGenerator caching - Salvatore - Due: Oct 9
- [x] Split CSV date columns - Dmitrii - Due: Oct 10
- [x] Add XML comments to PageModel handlers - Backend team - Due: Oct 11
- [x] Write unit tests for ticket claiming - Salvatore - Due: Oct 12
- [x] Review all open PRs - All - Due: Oct 10

## Next Meeting
- **Date:** 2025-10-15
- **Time:** 14:00
- **Agenda:** Sprint 2 review and retrospective

## Notes
- Team morale is high, everyone feels confident about sprint completion
- Discussed need for better documentation of backend APIs
- Agreed to continue async daily standups in Discord
- Will schedule extra pairing session for CSV export testing
