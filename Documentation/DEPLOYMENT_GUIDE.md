# Campus Events System - Deployment Guide

## Overview

This guide provides comprehensive instructions for deploying the Campus Events system to various environments, from development to production.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Development Setup](#development-setup)
3. [Production Deployment](#production-deployment)
4. [Database Migration](#database-migration)
5. [Configuration Management](#configuration-management)
6. [Security Considerations](#security-considerations)
7. [Monitoring and Logging](#monitoring-and-logging)
8. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Software

- **.NET 9.0 SDK**: Latest version from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)
- **Git**: For version control
- **SQLite**: Included with .NET (for development)
- **SQL Server** or **PostgreSQL**: For production (recommended)

### Optional Tools

- **Visual Studio 2022** or **VS Code**: IDE
- **Entity Framework Core Tools**: For database migrations
  ```bash
  dotnet tool install --global dotnet-ef
  ```

### System Requirements

#### Development
- **OS**: Windows, macOS, or Linux
- **RAM**: 4GB minimum, 8GB recommended
- **Disk**: 500MB free space
- **CPU**: Any modern processor

#### Production
- **OS**: Windows Server 2019+, Linux (Ubuntu 20.04+), or macOS Server
- **RAM**: 2GB minimum, 4GB+ recommended
- **Disk**: 10GB+ free space (depends on database size)
- **CPU**: 2+ cores recommended

---

## Development Setup

### 1. Clone Repository

```bash
git clone https://github.com/your-org/MONDAY_FK-X-SOEN341_PROJECT_F25.git
cd MONDAY_FK-X-SOEN341_PROJECT_F25
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Configure Database

#### SQLite (Default - Development)

The application uses SQLite by default. No additional configuration needed.

#### SQL Server (Optional - Development)

Update `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CampusEvents;User Id=sa;Password=YourPassword;TrustServerCertificate=true"
  }
}
```

Update `Program.cs` to use SQL Server:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### 4. Run Database Migrations

```bash
# Create database and apply migrations
dotnet ef database update
```

### 5. Configure Security Keys

#### Development (appsettings.Development.json)

```json
{
  "Security": {
    "TicketSigningKey": "your-development-signing-key-minimum-32-characters-long",
    "EncryptionKey": "your-development-encryption-key-minimum-32-characters-long"
  }
}
```

Generate keys:

```bash
# Generate signing key
openssl rand -base64 48

# Generate encryption key
openssl rand -base64 48
```

### 6. Run Application

```bash
# Development with hot reload
dotnet watch run

# Or standard run
dotnet run
```

### 7. Access Application

- **HTTP**: http://localhost:5136
- **HTTPS**: https://localhost:7295 (requires certificate)

### 8. Initial Admin Account

The application automatically creates an admin account on first run:

- **Email**: admin@campusevents.com
- **Password**: Admin@123

**⚠️ IMPORTANT**: Change this password immediately after first login!

---

## Production Deployment

### Option 1: Self-Hosted (Kestrel)

#### Windows

1. **Publish Application**

```bash
dotnet publish -c Release -o ./publish
```

2. **Configure appsettings.Production.json**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=CampusEvents;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=true"
  },
  "Security": {
    "TicketSigningKey": "PRODUCTION_SIGNING_KEY_FROM_ENVIRONMENT_VARIABLE",
    "EncryptionKey": "PRODUCTION_ENCRYPTION_KEY_FROM_ENVIRONMENT_VARIABLE"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

3. **Set Environment Variables**

```powershell
# Windows PowerShell
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ASPNETCORE_URLS = "http://localhost:5000;https://localhost:5001"
$env:SECURITY__TICKETSIGNINGKEY = "your-production-signing-key"
$env:SECURITY__ENCRYPTIONKEY = "your-production-encryption-key"
```

4. **Run Application**

```bash
cd publish
dotnet CampusEvents.dll
```

#### Linux

1. **Publish Application**

```bash
dotnet publish -c Release -o ./publish
```

2. **Create Systemd Service**

Create `/etc/systemd/system/campusevents.service`:

```ini
[Unit]
Description=Campus Events Application
After=network.target

[Service]
Type=notify
User=www-data
WorkingDirectory=/var/www/campusevents
ExecStart=/usr/bin/dotnet /var/www/campusevents/CampusEvents.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=campusevents
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000

[Install]
WantedBy=multi-user.target
```

3. **Enable and Start Service**

```bash
sudo systemctl enable campusevents
sudo systemctl start campusevents
sudo systemctl status campusevents
```

### Option 2: IIS (Windows)

1. **Install IIS and ASP.NET Core Hosting Bundle**

Download from: https://dotnet.microsoft.com/download/dotnet/9.0

2. **Publish Application**

```bash
dotnet publish -c Release -o C:\inetpub\wwwroot\campusevents
```

3. **Create IIS Application Pool**

- Open IIS Manager
- Create new Application Pool:
  - Name: CampusEventsAppPool
  - .NET CLR Version: No Managed Code
  - Managed Pipeline Mode: Integrated

4. **Create IIS Website**

- Right-click Sites → Add Website
- Site name: CampusEvents
- Application pool: CampusEventsAppPool
- Physical path: C:\inetpub\wwwroot\campusevents
- Binding: HTTP (port 80) or HTTPS (port 443)

5. **Configure web.config** (auto-generated)

The publish process creates `web.config` automatically.

6. **Set Environment Variables**

In Application Pool → Advanced Settings:
- Set `ASPNETCORE_ENVIRONMENT` to `Production`

### Option 3: Docker

1. **Create Dockerfile**

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["CampusEvents.csproj", "./"]
RUN dotnet restore "CampusEvents.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "CampusEvents.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CampusEvents.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CampusEvents.dll"]
```

2. **Build Docker Image**

```bash
docker build -t campusevents:latest .
```

3. **Run Container**

```bash
docker run -d \
  --name campusevents \
  -p 80:80 \
  -p 443:443 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e SECURITY__TICKETSIGNINGKEY=your-key \
  -e SECURITY__ENCRYPTIONKEY=your-key \
  -v /path/to/database:/app/data \
  campusevents:latest
```

### Option 4: Cloud Platforms

#### Azure App Service

1. **Create App Service**

```bash
az webapp create \
  --resource-group YourResourceGroup \
  --plan YourAppServicePlan \
  --name YourAppName \
  --runtime "DOTNET|9.0"
```

2. **Deploy Application**

```bash
az webapp deployment source config-zip \
  --resource-group YourResourceGroup \
  --name YourAppName \
  --src ./publish.zip
```

3. **Configure Connection String**

```bash
az webapp config connection-string set \
  --resource-group YourResourceGroup \
  --name YourAppName \
  --connection-string-type SQLServer \
  --settings DefaultConnection="Server=...;Database=...;..."
```

4. **Set App Settings**

```bash
az webapp config appsettings set \
  --resource-group YourResourceGroup \
  --name YourAppName \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    SECURITY__TICKETSIGNINGKEY=your-key \
    SECURITY__ENCRYPTIONKEY=your-key
```

#### AWS Elastic Beanstalk

1. **Install EB CLI**

```bash
pip install awsebcli
```

2. **Initialize EB**

```bash
eb init -p "64bit Amazon Linux 2 v2.5.0 running .NET Core" campusevents
```

3. **Create Environment**

```bash
eb create campusevents-prod
```

4. **Configure Environment Variables**

```bash
eb setenv \
  ASPNETCORE_ENVIRONMENT=Production \
  SECURITY__TICKETSIGNINGKEY=your-key \
  SECURITY__ENCRYPTIONKEY=your-key
```

---

## Database Migration

### From SQLite to SQL Server

1. **Update Connection String**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=CampusEvents;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=true"
  }
}
```

2. **Update Program.cs**

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

3. **Install SQL Server Provider**

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

4. **Export SQLite Data** (if needed)

```bash
# Use SQLite tools to export data
sqlite3 campusevents.db .dump > data.sql
```

5. **Run Migrations**

```bash
dotnet ef database update
```

6. **Import Data** (if needed)

- Convert SQLite SQL to SQL Server format
- Import using SQL Server Management Studio or `sqlcmd`

---

## Configuration Management

### Environment Variables

#### Development

Set in `appsettings.Development.json` or User Secrets:

```bash
dotnet user-secrets set "Security:TicketSigningKey" "your-dev-key"
dotnet user-secrets set "Security:EncryptionKey" "your-dev-key"
```

#### Production

Use environment variables (recommended):

```bash
# Linux/macOS
export SECURITY__TICKETSIGNINGKEY="your-production-key"
export SECURITY__ENCRYPTIONKEY="your-production-key"

# Windows
set SECURITY__TICKETSIGNINGKEY=your-production-key
set SECURITY__ENCRYPTIONKEY=your-production-key
```

### Azure Key Vault (Recommended for Production)

1. **Install Package**

```bash
dotnet add package Azure.Extensions.AspNetCore.Configuration.Secrets
dotnet add package Azure.Identity
```

2. **Configure in Program.cs**

```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"),
    new DefaultAzureCredential());
```

### Configuration Priority

1. Environment variables (highest priority)
2. User Secrets (development)
3. appsettings.{Environment}.json
4. appsettings.json (lowest priority)

---

## Security Considerations

### 1. HTTPS Configuration

#### Development

```bash
dotnet dev-certs https --trust
```

#### Production

- Use valid SSL certificate
- Configure HSTS headers
- Redirect HTTP to HTTPS

### 2. Security Headers

Add to `Program.cs`:

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'");
    await next();
});
```

### 3. Key Management

- **Never commit keys to version control**
- Use environment variables or secure key storage
- Rotate keys periodically
- Use different keys for each environment

### 4. Database Security

- Use strong passwords
- Limit database user permissions
- Enable encryption at rest
- Use parameterized queries (already implemented)

### 5. Session Security

- Use secure cookies in production
- Set appropriate session timeout
- Consider using Redis for distributed sessions

---

## Monitoring and Logging

### Application Insights (Azure)

1. **Install Package**

```bash
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

2. **Configure in Program.cs**

```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

3. **Set Instrumentation Key**

```bash
az webapp config appsettings set \
  --name YourAppName \
  --resource-group YourResourceGroup \
  --settings ApplicationInsights_InstrumentationKey=your-key
```

### Serilog (Alternative)

1. **Install Packages**

```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
dotnet add package Serilog.Sinks.Console
```

2. **Configure in Program.cs**

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
```

### Health Checks

1. **Add Health Checks**

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

app.MapHealthChecks("/health");
```

2. **Monitor Health Endpoint**

```bash
curl https://your-app.com/health
```

---

## Troubleshooting

### Common Issues

#### 1. Database Connection Errors

**Symptoms**: Application fails to start, database errors in logs

**Solutions**:
- Verify connection string is correct
- Check database server is running
- Verify user permissions
- Check firewall rules

#### 2. Migration Errors

**Symptoms**: `dotnet ef database update` fails

**Solutions**:
- Ensure database exists
- Check user has CREATE/ALTER permissions
- Review migration files for errors
- Try dropping and recreating database (development only)

#### 3. Security Key Errors

**Symptoms**: "TicketSigningKey not configured" error

**Solutions**:
- Verify keys are set in configuration or environment variables
- Check key length (minimum 32 characters)
- Verify environment variable naming (use `__` for nested keys)

#### 4. Port Already in Use

**Symptoms**: "Address already in use" error

**Solutions**:
- Change port in `launchSettings.json`
- Kill process using the port
- Use different port via command line: `dotnet run --urls "http://localhost:5000"`

#### 5. SSL Certificate Issues

**Symptoms**: HTTPS not working, certificate errors

**Solutions**:
- Trust development certificate: `dotnet dev-certs https --trust`
- For production, use valid SSL certificate
- Configure certificate in IIS or reverse proxy

### Logging

Enable detailed logging for troubleshooting:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

### Performance Issues

1. **Database Queries**
   - Enable query logging
   - Review slow queries
   - Add indexes if needed

2. **Memory Usage**
   - Monitor application memory
   - Check for memory leaks
   - Review caching strategies

3. **Response Times**
   - Use Application Insights or similar
   - Profile slow endpoints
   - Optimize database queries

---

## Backup and Recovery

### Database Backups

#### SQLite

```bash
# Simple file copy
cp campusevents.db campusevents.db.backup
```

#### SQL Server

```sql
BACKUP DATABASE CampusEvents
TO DISK = 'C:\Backups\CampusEvents.bak'
WITH FORMAT, COMPRESSION;
```

### Automated Backups

Set up scheduled backups:

- **Windows**: Task Scheduler
- **Linux**: Cron job
- **Cloud**: Use platform backup services

### Recovery Procedures

1. **Stop Application**
2. **Restore Database**
3. **Verify Data Integrity**
4. **Restart Application**
5. **Test Functionality**

---

## Scaling

### Horizontal Scaling

1. **Load Balancer**: Distribute traffic across multiple instances
2. **Session State**: Use Redis or SQL Server for shared sessions
3. **Database**: Use read replicas for reporting queries

### Vertical Scaling

1. **Increase Server Resources**: CPU, RAM, Disk
2. **Optimize Database**: Indexes, query optimization
3. **Caching**: Implement caching for frequently accessed data

---

## Maintenance

### Regular Tasks

1. **Database Maintenance**
   - Regular backups
   - Index optimization
   - Cleanup old data

2. **Security Updates**
   - Update .NET runtime
   - Update dependencies
   - Review security advisories

3. **Monitoring**
   - Review logs
   - Check performance metrics
   - Monitor error rates

---

**Document Version**: 1.0
**Last Updated**: 2025-01-XX
**Maintained By**: Development Team

