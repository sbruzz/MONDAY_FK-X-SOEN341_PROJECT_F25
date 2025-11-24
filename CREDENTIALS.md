# Login Credentials

**Status:** ✅ Database verified and working (Last updated: 2025-11-23)

**All passwords:** `Demo@123` (except Admin which is `Admin@123`)

---

## Quick Test Accounts

**Student:** `alice.smith@student.concordia.ca` / `Demo@123`

**Organizer:** `tech.club@concordia.ca` / `Demo@123`

**Admin:** `admin@campusevents.com` / `Admin@123`

---

## Admin Account
- **Email:** `admin@campusevents.com`
- **Password:** `Admin@123`
- **Role:** Administrator (full access)

---

## Organizers (Approved - Can Login)
All use password: `Demo@123`

| Email | Name | Department | Organization |
|-------|------|------------|--------------|
| `tech.club@concordia.ca` | Sarah Martinez | Computer Science | Tech Innovators Club |
| `music.society@concordia.ca` | Mike Chen | Fine Arts | Concordia Music Society |
| `sports.association@concordia.ca` | Jennifer Lee | Athletics | Sports & Wellness Association |

---

## Organizers (Pending Approval - Cannot Login)
These accounts exist but **cannot login** until admin approves them:

| Email | Name | Status |
|-------|------|--------|
| `new.club@concordia.ca` | Tom Wilson | ⏳ Pending |
| `pending.org@concordia.ca` | Lisa Garcia | ⏳ Pending |

---

## Students (All Can Login)
All use password: `Demo@123`

| Email | Name | Program | Year |
|-------|------|---------|------|
| `alice.smith@student.concordia.ca` | Alice Smith | Computer Science | 3rd Year |
| `bob.johnson@student.concordia.ca` | Bob Johnson | Engineering | 2nd Year |
| `carol.williams@student.concordia.ca` | Carol Williams | Business | 4th Year |
| `david.brown@student.concordia.ca` | David Brown | Computer Science | 1st Year |
| `eve.davis@student.concordia.ca` | Eve Davis | Arts | 2nd Year |

---

## Troubleshooting

### If login fails with "Invalid email or password":

1. **Run the database fix script:**
   ```bash
   ./fix_database.sh
   ```
   This will delete the old database and create a fresh one with correct password hashes.

2. **Manual database rebuild:**
   ```bash
   pkill -f dotnet
   rm -f campusevents.db campusevents.db-shm campusevents.db-wal
   dotnet ef database update
   dotnet run
   ```

3. **Check the application is running:**
   - URL: http://localhost:5136
   - Look for "✅ Demo users created" in the console output

---

## Additional Seeded Data

The database also includes:
- 6 events (5 approved, 1 pending)
- 3 organizations
- 2 active drivers (+ 1 pending approval)
- 3 carpool offers
- 3 available rooms
- 5 room rentals (3 approved, 2 pending)
