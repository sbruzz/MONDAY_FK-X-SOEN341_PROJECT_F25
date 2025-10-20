#!/bin/bash

# Campus Events Development Script
# This script runs both the ASP.NET Core backend and Next.js frontend

echo "🚀 Starting Campus Events Development Environment"
echo "================================================"

# Function to kill background processes on exit
cleanup() {
    echo "🛑 Shutting down development servers..."
    kill $BACKEND_PID $FRONTEND_PID 2>/dev/null
    exit
}

# Set up signal handlers
trap cleanup SIGINT SIGTERM

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET is not installed. Please install .NET 9.0 SDK"
    exit 1
fi

# Check if Node.js is installed
if ! command -v node &> /dev/null; then
    echo "❌ Node.js is not installed. Please install Node.js 18+"
    exit 1
fi

# Check if npm is installed
if ! command -v npm &> /dev/null; then
    echo "❌ npm is not installed. Please install npm"
    exit 1
fi

echo "✅ Prerequisites check passed"

# Install frontend dependencies if needed
if [ ! -d "frontend/node_modules" ]; then
    echo "📦 Installing frontend dependencies..."
    cd frontend
    npm install
    cd ..
fi

# Start the ASP.NET Core backend
echo "🔧 Starting ASP.NET Core backend on port 5000..."
dotnet run --urls="http://localhost:5000" &
BACKEND_PID=$!

# Wait a moment for backend to start
sleep 3

# Start the Next.js frontend
echo "⚛️  Starting Next.js frontend on port 3000..."
cd frontend
npm run dev &
FRONTEND_PID=$!
cd ..

echo ""
echo "🎉 Development environment is ready!"
echo "================================================"
echo "📱 Frontend: http://localhost:3000"
echo "🔧 Backend API: http://localhost:5000/api"
echo "📊 Backend Pages: http://localhost:5000"
echo ""
echo "Press Ctrl+C to stop both servers"
echo ""

# Wait for user to stop
wait
