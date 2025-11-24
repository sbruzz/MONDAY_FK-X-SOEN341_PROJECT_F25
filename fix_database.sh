#!/bin/bash

echo "🔧 Fixing database..."

# Kill any running dotnet processes
echo "1. Killing dotnet processes..."
pkill -f dotnet
sleep 2

# Delete database files
echo "2. Deleting old database..."
rm -f campusevents.db
rm -f campusevents.db-shm
rm -f campusevents.db-wal

# Verify deletion
if [ -f "campusevents.db" ]; then
    echo "❌ ERROR: Database file still exists!"
    echo "Try: sudo rm -f campusevents.db"
    exit 1
fi

echo "✅ Database deleted"

# Rebuild database
echo "3. Running migrations..."
dotnet ef database update

# Verify database was created
if [ ! -f "campusevents.db" ]; then
    echo "❌ ERROR: Database was not created!"
    exit 1
fi

echo "✅ Database created"
echo ""
echo "4. Starting application..."
echo "   Watch for '✅ Demo users created' message"
echo ""
dotnet run
