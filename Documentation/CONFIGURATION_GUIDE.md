# Campus Events System - Configuration Guide

## Overview

This guide provides comprehensive information about configuring the Campus Events system for different environments and scenarios.

## Table of Contents

1. [Configuration Files](#configuration-files)
2. [Connection Strings](#connection-strings)
3. [Security Configuration](#security-configuration)
4. [Application Settings](#application-settings)
5. [Environment-Specific Configuration](#environment-specific-configuration)
6. [Key Management](#key-management)

---

## Configuration Files

### appsettings.json

Base configuration file for all environments.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=campusevents.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### appsettings.Development.json

Development-specific configuration (overrides appsettings.json).

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=campusevents.db"
  },
  "Security": {
    "TicketSigningKey": "development-signing-key-minimum-32-characters-long",
    "EncryptionKey": "development-encryption-key-minimum-32-characters-long"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

### appsettings.Production.json

Production-specific configuration (overrides appsettings.json).

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=PRODUCTION_SERVER;Database=CampusEvents;..."
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Error"
    }
  }
}
```

---

## Connection Strings

### SQLite (Development)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=campusevents.db"
  }
}
```

**Features**:
- File-based database
- No server required
- Good for development
- Portable database file

### SQL Server

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CampusEvents;User Id=sa;Password=YourPassword;TrustServerCertificate=true"
  }
}
```

**Features**:
- Production-ready
- High performance
- Advanced features
- Requires SQL Server installation

### PostgreSQL

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=CampusEvents;Username=postgres;Password=YourPassword"
  }
}
```

**Features**:
- Open source
- Cross-platform
- Production-ready
- Requires PostgreSQL installation

---

## Security Configuration

### Ticket Signing Key

**Purpose**: HMAC-SHA256 key for signing ticket QR codes.

**Configuration**:
```json
{
  "Security": {
    "TicketSigningKey": "your-signing-key-minimum-32-characters-long"
  }
}
```

**Environment Variable**:
```bash
export SECURITY__TICKETSIGNINGKEY="your-production-signing-key"
```

**Generation**:
```bash
openssl rand -base64 48
```

**Requirements**:
- Minimum 32 characters
- Cryptographically secure random
- Different for each environment
- Never commit to version control

### Encryption Key

**Purpose**: AES-256 key for encrypting sensitive data.

**Configuration**:
```json
{
  "Security": {
    "EncryptionKey": "your-encryption-key-minimum-32-characters-long"
  }
}
```

**Environment Variable**:
```bash
export SECURITY__ENCRYPTIONKEY="your-production-encryption-key"
```

**Generation**:
```bash
openssl rand -base64 32
```

**Requirements**:
- Minimum 32 characters
- Cryptographically secure random
- Different for each environment
- Never commit to version control

---

## Application Settings

### Session Configuration

```json
{
  "Session": {
    "TimeoutMinutes": 30,
    "CookieName": ".CampusEvents.Session"
  }
}
```

**Settings**:
- `TimeoutMinutes`: Session idle timeout (default: 30)
- `CookieName`: Session cookie name

### Logging Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

**Log Levels**:
- `Trace`: Very detailed logs
- `Debug`: Debug information
- `Information`: General information
- `Warning`: Warning messages
- `Error`: Error messages
- `Critical`: Critical errors

### File Upload Configuration

```json
{
  "FileUpload": {
    "MaxFileSize": 10485760,
    "AllowedExtensions": [".csv", ".jpg", ".png"]
  }
}
```

**Settings**:
- `MaxFileSize`: Maximum file size in bytes (default: 10MB)
- `AllowedExtensions`: Allowed file extensions

---

## Environment-Specific Configuration

### Development

**File**: `appsettings.Development.json`

**Settings**:
- Detailed logging enabled
- Sensitive data logging enabled
- Development database (SQLite)
- Development security keys
- Hot reload enabled

### Production

**File**: `appsettings.Production.json`

**Settings**:
- Minimal logging (Warnings and Errors only)
- Production database (SQL Server/PostgreSQL)
- Security keys from environment variables
- HTTPS enforced
- Security headers enabled

### Staging

**File**: `appsettings.Staging.json`

**Settings**:
- Moderate logging
- Staging database
- Staging security keys
- Similar to production but with more logging

---

## Key Management

### Development Keys

**Location**: `appsettings.Development.json` or User Secrets

**User Secrets**:
```bash
dotnet user-secrets set "Security:TicketSigningKey" "dev-key"
dotnet user-secrets set "Security:EncryptionKey" "dev-key"
```

### Production Keys

**Location**: Environment variables or Azure Key Vault

**Environment Variables**:
```bash
export SECURITY__TICKETSIGNINGKEY="production-key"
export SECURITY__ENCRYPTIONKEY="production-key"
```

**Azure Key Vault**:
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{vaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

### Key Rotation

**Process**:
1. Generate new keys
2. Update configuration
3. Re-encrypt existing data (if needed)
4. Deploy updated configuration
5. Verify system functionality

**Note**: Ticket signing keys can be rotated without data migration.
Encryption keys require re-encryption of all encrypted data.

---

## Configuration Priority

Configuration is loaded in the following order (later overrides earlier):

1. `appsettings.json` (base configuration)
2. `appsettings.{Environment}.json` (environment-specific)
3. User Secrets (development only)
4. Environment variables (highest priority)

### Example

```json
// appsettings.json
{
  "Security": {
    "TicketSigningKey": "base-key"
  }
}

// appsettings.Development.json
{
  "Security": {
    "TicketSigningKey": "dev-key"  // Overrides base
  }
}

// Environment variable
SECURITY__TICKETSIGNINGKEY=env-key  // Overrides all
```

---

## Best Practices

### 1. Never Commit Secrets

- Use `.gitignore` for sensitive files
- Use environment variables for production
- Use User Secrets for development

### 2. Use Different Keys Per Environment

- Development: `dev-key-...`
- Staging: `staging-key-...`
- Production: `prod-key-...`

### 3. Rotate Keys Regularly

- Quarterly key rotation recommended
- Document rotation process
- Test rotation in staging first

### 4. Monitor Configuration

- Log configuration loading (without secrets)
- Alert on missing configuration
- Validate configuration on startup

---

**Document Version**: 1.0
**Last Updated**: 2025-01-XX
**Maintained By**: Development Team

