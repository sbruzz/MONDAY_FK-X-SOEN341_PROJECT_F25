# Campus Events System - Performance Guide

## Overview

This guide provides comprehensive information about performance optimization, monitoring, and best practices for the Campus Events system.

## Table of Contents

1. [Performance Metrics](#performance-metrics)
2. [Database Optimization](#database-optimization)
3. [Query Optimization](#query-optimization)
4. [Caching Strategies](#caching-strategies)
5. [Application Performance](#application-performance)
6. [Frontend Performance](#frontend-performance)
7. [Monitoring and Profiling](#monitoring-and-profiling)

---

## Performance Metrics

### Key Performance Indicators (KPIs)

#### Response Time Targets
- **Page Load**: < 2 seconds
- **API Endpoints**: < 500ms
- **Database Queries**: < 100ms
- **QR Code Generation**: < 50ms

#### Throughput Targets
- **Concurrent Users**: 100+ simultaneous users
- **Requests per Second**: 50+ RPS
- **Database Connections**: Efficient connection pooling

#### Resource Usage
- **CPU**: < 70% average
- **Memory**: < 2GB for typical deployment
- **Database Size**: Scalable to 10GB+

---

## Database Optimization

### Indexing Strategy

#### Primary Keys
All entities have auto-increment integer primary keys, which are automatically indexed.

#### Unique Indexes
```sql
-- User email (frequently queried)
CREATE UNIQUE INDEX IX_Users_Email ON Users(Email);

-- Ticket unique code (validation queries)
CREATE UNIQUE INDEX IX_Tickets_UniqueCode ON Tickets(UniqueCode);
```

#### Composite Indexes
```sql
-- Room rental overlap detection (critical for performance)
CREATE INDEX IX_RoomRentals_RoomId_StartTime_EndTime 
ON RoomRentals(RoomId, StartTime, EndTime);

-- Notification queries (frequently filtered)
CREATE INDEX IX_Notifications_UserId_IsRead_CreatedAt 
ON Notifications(UserId, IsRead, CreatedAt);
```

#### Foreign Key Indexes
Entity Framework automatically indexes foreign keys, but verify:
- UserId (in Tickets, SavedEvents, etc.)
- EventId (in Tickets, CarpoolOffers)
- OrganizerId (in Events, Rooms)

### Query Optimization

#### Eager Loading

**Problem**: N+1 Query Problem
```csharp
// ❌ Bad: N+1 queries
var events = await _context.Events.ToListAsync();
foreach (var event in events)
{
    var organizer = await _context.Users.FindAsync(event.OrganizerId); // N queries!
}
```

**Solution**: Use Include()
```csharp
// ✅ Good: Single query with join
var events = await _context.Events
    .Include(e => e.Organizer)
    .Include(e => e.Organization)
    .ToListAsync();
```

#### Projection Queries

**Problem**: Loading unnecessary data
```csharp
// ❌ Bad: Loads entire entities
var events = await _context.Events.ToListAsync();
var summaries = events.Select(e => new { e.Id, e.Title });
```

**Solution**: Project directly
```csharp
// ✅ Good: Only loads needed data
var summaries = await _context.Events
    .Select(e => new { e.Id, e.Title, e.EventDate })
    .ToListAsync();
```

#### AsNoTracking()

**Problem**: Unnecessary change tracking
```csharp
// ❌ Bad: Tracks entities unnecessarily
var events = await _context.Events.ToListAsync(); // Read-only but tracked
```

**Solution**: Use AsNoTracking()
```csharp
// ✅ Good: No change tracking overhead
var events = await _context.Events
    .AsNoTracking()
    .ToListAsync();
```

### Database Connection Management

#### Connection Pooling
Entity Framework manages connection pooling automatically:
- Connections are reused from pool
- Pool size adjusts based on demand
- Connections released back to pool after use

#### Best Practices
```csharp
// ✅ Good: Let EF manage connections
using var context = new AppDbContext(options);
var events = await context.Events.ToListAsync();
// Connection automatically released

// ❌ Bad: Manual connection management
var connection = new SqliteConnection(...);
// Manual management not needed with EF Core
```

---

## Query Optimization

### Filtering Early

```csharp
// ✅ Good: Filter before loading
var activeEvents = await _context.Events
    .Where(e => e.ApprovalStatus == ApprovalStatus.Approved)
    .Where(e => e.EventDate > DateTime.UtcNow)
    .ToListAsync();

// ❌ Bad: Filter after loading
var allEvents = await _context.Events.ToListAsync();
var activeEvents = allEvents
    .Where(e => e.ApprovalStatus == ApprovalStatus.Approved)
    .ToList(); // Filtered in memory
```

### Using Query Helpers

```csharp
// Use QueryHelper extension methods for consistent filtering
var events = await _context.Events
    .FilterByApprovalStatus(ApprovalStatus.Approved)
    .FilterFutureEvents()
    .FilterByCategory(EventCategory.Social)
    .OrderByDate()
    .ToListAsync();
```

### Pagination

```csharp
// Always paginate large result sets
var events = await _context.Events
    .FilterByApprovalStatus(ApprovalStatus.Approved)
    .Paginate(pageNumber: 1, pageSize: 20)
    .ToListAsync();
```

---

## Caching Strategies

### In-Memory Caching

#### CacheHelper Usage
```csharp
// Cache frequently accessed, rarely changed data
var organizations = CacheHelper.GetOrSet(
    "organizations",
    () => _context.Organizations.ToList(),
    expirationMinutes: 60
);
```

#### What to Cache
- ✅ Organization list (rarely changes)
- ✅ Event categories (static)
- ✅ User roles (static)
- ❌ User-specific data (changes frequently)
- ❌ Event lists (changes frequently)

### Cache Invalidation
```csharp
// Invalidate cache when data changes
public async Task CreateOrganizationAsync(Organization org)
{
    _context.Organizations.Add(org);
    await _context.SaveChangesAsync();
    
    // Invalidate cache
    CacheHelper.Remove("organizations");
}
```

### Production Caching

For production, use distributed caching:
```csharp
// Redis or SQL Server cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});
```

---

## Application Performance

### Async/Await

**Always use async for I/O operations**:
```csharp
// ✅ Good: Async I/O
public async Task<List<Event>> GetEventsAsync()
{
    return await _context.Events.ToListAsync();
}

// ❌ Bad: Synchronous I/O
public List<Event> GetEvents()
{
    return _context.Events.ToList(); // Blocks thread
}
```

### Service Lifetime

Choose appropriate service lifetimes:
- **Singleton**: Stateless services (TicketSigningService, EncryptionService)
- **Scoped**: Per-request services (CarpoolService, RoomRentalService)
- **Transient**: Lightweight services (DbCSVCommunicator)

### Memory Management

#### Dispose Resources
```csharp
// ✅ Good: Using statement ensures disposal
using var context = new AppDbContext(options);
var events = await context.Events.ToListAsync();
// Context automatically disposed

// ✅ Good: DI handles disposal automatically
public class MyService
{
    private readonly AppDbContext _context;
    // Disposed automatically at end of request
}
```

---

## Frontend Performance

### Asset Optimization

#### Minification
```bash
# Minify CSS and JavaScript in production
# Use build tools or CDN
```

#### Compression
Enable gzip compression in production:
```csharp
app.UseResponseCompression();
```

### Lazy Loading

#### Defer Non-Critical JavaScript
```html
<script src="script.js" defer></script>
```

#### Lazy Load Images
```html
<img src="image.jpg" loading="lazy" />
```

### Pagination

Always paginate large lists:
```csharp
// Server-side pagination
var events = await _context.Events
    .Paginate(pageNumber, pageSize)
    .ToListAsync();
```

---

## Monitoring and Profiling

### Application Insights

```csharp
builder.Services.AddApplicationInsightsTelemetry();

// Track custom events
_telemetryClient.TrackEvent("TicketClaimed", new Dictionary<string, string>
{
    { "EventId", eventId.ToString() },
    { "UserId", userId.ToString() }
});
```

### Performance Counters

Monitor:
- Request duration
- Database query time
- Memory usage
- CPU usage
- Error rates

### Logging

```csharp
// Log slow operations
var stopwatch = Stopwatch.StartNew();
await ProcessOperation();
stopwatch.Stop();

if (stopwatch.ElapsedMilliseconds > 1000)
{
    LoggingHelper.LogWarning(
        $"Slow operation: {stopwatch.ElapsedMilliseconds}ms",
        "Performance"
    );
}
```

---

## Performance Best Practices

### 1. Database Queries

- ✅ Use Include() for related data
- ✅ Use AsNoTracking() for read-only queries
- ✅ Filter before loading
- ✅ Use pagination for large result sets
- ✅ Project only needed fields
- ❌ Avoid N+1 queries
- ❌ Don't load unnecessary data

### 2. Caching

- ✅ Cache static/semi-static data
- ✅ Set appropriate expiration times
- ✅ Invalidate cache on updates
- ❌ Don't cache user-specific data
- ❌ Don't cache frequently changing data

### 3. Async Operations

- ✅ Use async/await for all I/O
- ✅ Use ConfigureAwait(false) in library code
- ❌ Don't use async void
- ❌ Don't block async operations

### 4. Memory Management

- ✅ Dispose resources properly
- ✅ Use using statements
- ✅ Let DI handle disposal
- ❌ Don't hold references unnecessarily
- ❌ Don't create memory leaks

---

## Performance Testing

### Load Testing

Use tools like:
- Apache JMeter
- k6
- Azure Load Testing

### Stress Testing

Test system under:
- High concurrent users
- Large data volumes
- Peak traffic scenarios

### Benchmarking

Measure:
- Response times
- Throughput
- Resource usage
- Error rates

---

## Troubleshooting Performance Issues

### Slow Queries

1. Enable query logging
2. Identify slow queries
3. Add indexes if needed
4. Optimize query logic
5. Consider caching

### High Memory Usage

1. Check for memory leaks
2. Review caching strategy
3. Optimize data loading
4. Consider pagination
5. Monitor object lifetimes

### High CPU Usage

1. Profile CPU-intensive operations
2. Optimize algorithms
3. Consider async operations
4. Review service lifetimes
5. Check for blocking operations

---

**Document Version**: 1.0
**Last Updated**: 2025-01-XX
**Maintained By**: Development Team

