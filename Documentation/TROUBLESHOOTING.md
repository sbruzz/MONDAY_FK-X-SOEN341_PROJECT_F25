# Campus Events System - Troubleshooting Guide

## Overview

This guide helps diagnose and resolve common issues encountered when developing, deploying, or using the Campus Events system.

## Table of Contents

1. [Database Issues](#database-issues)
2. [Authentication Problems](#authentication-problems)
3. [Performance Issues](#performance-issues)
4. [Build and Compilation Errors](#build-and-compilation-errors)
5. [Runtime Errors](#runtime-errors)
6. [Migration Issues](#migration-issues)
7. [Service Registration Problems](#service-registration-problems)

---

## Database Issues

### Database Not Found

**Symptoms**: Application fails to start, "database not found" error

**Solutions**:
1. Check if `campusevents.db` exists in the project root
2. Run database migrations: `dotnet ef database update`
3. Check connection string in `appsettings.json`
4. Verify database file permissions

### Migration Errors

**Symptoms**: Migration fails, foreign key errors, constraint violations

**Solutions**:
1. Check for pending migrations: `dotnet ef migrations list`
2. Apply migrations in order: `dotnet ef database update`
3. If stuck, reset database: `dotnet ef database drop` then `dotnet ef database update`
4. Check migration files for syntax errors
5. Verify model changes match migration changes

### SQLite Locked Database

**Symptoms**: "database is locked" error, concurrent access issues

**Solutions**:
1. SQLite has single-writer limitation
2. Ensure only one process accesses database at a time
3. Close database connections properly
4. Use connection pooling appropriately
5. Consider upgrading to SQL Server for production

### Foreign Key Violations

**Symptoms**: Foreign key constraint errors when deleting/updating

**Solutions**:
1. Check for dependent records before deletion
2. Use cascade delete where appropriate
3. Delete dependent records first
4. Check `OnModelCreating` configuration

---

## Authentication Problems

### Login Not Working

**Symptoms**: Cannot log in, "invalid credentials" error

**Solutions**:
1. Verify password hashing: Check if using `PasswordHelper.HashPassword()`
2. Verify password verification: Check if using `PasswordHelper.VerifyPassword()`
3. Check session configuration in `Program.cs`
4. Verify user exists in database
5. Check password hash format (BCrypt)

### Session Not Persisting

**Symptoms**: User logged out after page refresh, session lost

**Solutions**:
1. Check session configuration in `Program.cs`
2. Verify session cookie settings
3. Check browser cookie settings
4. Verify session store is configured
5. Check session timeout settings

### Role-Based Access Not Working

**Symptoms**: Users can access unauthorized pages

**Solutions**:
1. Verify user role is set correctly in database
2. Check authorization attributes on pages
3. Verify role checking logic
4. Check session contains user role
5. Verify authorization policies in `Program.cs`

---

## Performance Issues

### Slow Database Queries

**Symptoms**: Pages load slowly, timeouts

**Solutions**:
1. Add database indexes for frequently queried fields
2. Use `Include()` for eager loading (avoid N+1 queries)
3. Use projections to limit data: `.Select(e => new { e.Id, e.Title })`
4. Implement pagination for large datasets
5. Use `QueryHelper` extensions for optimized queries
6. Consider caching frequently accessed data

### Memory Issues

**Symptoms**: High memory usage, out of memory errors

**Solutions**:
1. Use pagination instead of loading all records
2. Dispose of database contexts properly
3. Use `using` statements for resources
4. Check for memory leaks in event handlers
5. Limit result sets with `.Take()` or `.Skip()`

### Slow Page Loads

**Symptoms**: Pages take long to render

**Solutions**:
1. Optimize database queries (see above)
2. Enable response compression
3. Minimize JavaScript and CSS
4. Use async operations where possible
5. Implement caching for static data
6. Check for blocking operations

---

## Build and Compilation Errors

### Missing Dependencies

**Symptoms**: "Package not found" errors, missing references

**Solutions**:
1. Restore NuGet packages: `dotnet restore`
2. Check `CampusEvents.csproj` for package references
3. Verify package versions are compatible
4. Clear NuGet cache: `dotnet nuget locals all --clear`
5. Rebuild solution: `dotnet clean` then `dotnet build`

### Type Not Found Errors

**Symptoms**: "Type or namespace not found" errors

**Solutions**:
1. Check using statements
2. Verify namespace matches folder structure
3. Check project references
4. Rebuild solution
5. Check for typos in class/namespace names

### XML Documentation Warnings

**Symptoms**: Warnings about missing XML documentation

**Solutions**:
1. Add XML documentation comments to public members
2. Use `/// <summary>` tags
3. Document parameters with `/// <param name="paramName">`
4. Document return values with `/// <returns>`
5. Suppress warnings if not needed: `#pragma warning disable 1591`

---

## Runtime Errors

### Null Reference Exceptions

**Symptoms**: "Object reference not set to an instance" errors

**Solutions**:
1. Check for null before accessing properties
2. Use null-conditional operators: `user?.Email`
3. Use null-coalescing: `user?.Name ?? "Unknown"`
4. Verify database queries return expected data
5. Check for missing `Include()` statements

### Invalid Operation Exceptions

**Symptoms**: "Invalid operation" errors, context disposed errors

**Solutions**:
1. Check database context lifetime (should be scoped)
2. Don't use disposed contexts
3. Use async methods properly
4. Check for concurrent modifications
5. Verify service registration in `Program.cs`

### Argument Exceptions

**Symptoms**: "Argument is null" or "Invalid argument" errors

**Solutions**:
1. Validate input parameters
2. Use `ValidationHelper` for common validations
3. Check for null/empty strings
4. Verify parameter types match
5. Add parameter validation at method start

---

## Migration Issues

### Migration Not Applied

**Symptoms**: Database schema doesn't match models

**Solutions**:
1. Check migration status: `dotnet ef migrations list`
2. Apply pending migrations: `dotnet ef database update`
3. Verify migration files exist in `Migrations/` folder
4. Check migration order (should be chronological)

### Migration Conflicts

**Symptoms**: Multiple migrations with same timestamp

**Solutions**:
1. Remove duplicate migration files
2. Regenerate migration if needed: `dotnet ef migrations add MigrationName`
3. Check for merge conflicts in migration files
4. Verify migration history in database

### Model Changes Not Reflected

**Symptoms**: Model changes don't appear in database

**Solutions**:
1. Create new migration: `dotnet ef migrations add MigrationName`
2. Apply migration: `dotnet ef database update`
3. Check migration file includes model changes
4. Verify `OnModelCreating` configuration

---

## Service Registration Problems

### Service Not Found

**Symptoms**: "Service not registered" errors, dependency injection failures

**Solutions**:
1. Check service registration in `Program.cs`
2. Verify service lifetime (Singleton, Scoped, Transient)
3. Check service interface matches implementation
4. Verify service is registered before use
5. Check for circular dependencies

### Service Lifetime Issues

**Symptoms**: Context disposed errors, stale data

**Solutions**:
1. Use Scoped lifetime for DbContext
2. Use Transient for stateless services
3. Use Singleton only for thread-safe services
4. Don't inject Scoped services into Singleton services
5. Check service registration order

---

## Common Error Messages and Solutions

### "The database file is locked"

**Solution**: Close other connections, ensure single writer access

### "Foreign key constraint failed"

**Solution**: Delete dependent records first, or configure cascade delete

### "No service for type 'X' has been registered"

**Solution**: Register service in `Program.cs` with appropriate lifetime

### "Cannot access disposed object"

**Solution**: Check service lifetimes, don't use disposed contexts

### "Sequence contains no elements"

**Solution**: Check for empty collections, use `FirstOrDefault()` instead of `First()`

### "Invalid token"

**Solution**: Check ticket signing key, verify HMAC signature

### "Invalid license format"

**Solution**: Verify license format matches province/territory pattern

---

## Debugging Tips

### Enable Detailed Logging

1. Set log level to `Debug` in `appsettings.Development.json`
2. Check console output for detailed error messages
3. Use `LoggingHelper` for custom logging
4. Check database logs if available

### Use Breakpoints

1. Set breakpoints in problematic code
2. Step through execution
3. Inspect variable values
4. Check call stack

### Database Inspection

1. Use SQLite browser to inspect database
2. Check table contents
3. Verify relationships
4. Check for orphaned records

### Check Configuration

1. Verify `appsettings.json` settings
2. Check environment variables
3. Verify connection strings
4. Check service registrations

---

## Getting Help

### Before Asking for Help

1. Check this troubleshooting guide
2. Review error messages carefully
3. Check logs for detailed information
4. Verify configuration
5. Try common solutions

### When Reporting Issues

Include:
- Error message (full text)
- Stack trace
- Steps to reproduce
- Expected vs actual behavior
- Environment details (OS, .NET version)
- Relevant code snippets

---

**Document Version**: 1.0
**Last Updated**: 2025-01-XX
**Maintained By**: Development Team

