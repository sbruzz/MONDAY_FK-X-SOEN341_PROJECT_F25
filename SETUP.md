# Setup Instructions

## After Pulling New Code

If you're getting errors about missing tables (Rooms, Drivers, etc.), the database file might be outdated or locked.

### Option 1: Force Pull the Database (Easiest)
```bash
# Kill any running dotnet processes
pkill -f dotnet

# Delete your local database files
rm -f campusevents.db campusevents.db-shm campusevents.db-wal

# Force pull from git (this will get the latest database)
git checkout origin/feature/carpool-rental-clean -- campusevents.db

# Run the application
dotnet run
```

### Option 2: Rebuild Database from Migrations
```bash
# Delete database
rm -f campusevents.db campusevents.db-shm campusevents.db-wal

# Apply all migrations
dotnet ef database update

# Run the application
dotnet run
```

**Use Option 1** if you want the demo data.
**Use Option 2** if you want a fresh database.

The database will be automatically seeded with demo data including:
- Admin account: `admin@campusevents.com` / `Admin@123`
- Sample students, events, organizations, carpools, and rooms

## Running Tests
```bash
dotnet test
```

All 7 tests should pass.

## Common Issues

**Error: "no such table: Users"**
- Solution: You forgot step 1 & 2. Delete the database and run migrations.

**Error: "TicketSigningKey not configured"**
- Solution: The key is already in `appsettings.json`. Make sure you pulled the latest code.

**Port already in use**
- Solution: Kill the existing process or change the port in `Properties/launchSettings.json`
