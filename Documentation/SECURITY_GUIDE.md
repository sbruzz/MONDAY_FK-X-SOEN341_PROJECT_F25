# Campus Events System - Security Guide

## Overview

This document provides comprehensive security guidelines, best practices, and implementation details for the Campus Events system.

## Table of Contents

1. [Security Architecture](#security-architecture)
2. [Authentication & Authorization](#authentication--authorization)
3. [Data Protection](#data-protection)
4. [Input Validation](#input-validation)
5. [Secure Communication](#secure-communication)
6. [Security Best Practices](#security-best-practices)
7. [Vulnerability Management](#vulnerability-management)

---

## Security Architecture

### Defense in Depth

The application implements multiple layers of security:

1. **Network Layer**: HTTPS encryption, firewall rules
2. **Application Layer**: Authentication, authorization, input validation
3. **Data Layer**: Encryption at rest, parameterized queries
4. **Session Layer**: Secure session management, CSRF protection

### Security Principles

- **Least Privilege**: Users have minimum required permissions
- **Fail Secure**: System fails in secure state
- **Defense in Depth**: Multiple security layers
- **Security by Design**: Security built into architecture
- **Regular Updates**: Keep dependencies updated

---

## Authentication & Authorization

### Password Security

#### Hashing Algorithm
- **Algorithm**: BCrypt with adaptive work factor
- **Work Factor**: 12 (configurable)
- **Storage**: Hashed passwords only, never plain text

#### Password Requirements
- Minimum length: 8 characters
- Maximum length: 128 characters
- Must contain at least one letter
- Must contain at least one number
- Complexity requirements enforced

#### Password Storage
```csharp
// ✅ Good: Hash password before storage
var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
user.PasswordHash = passwordHash;

// ❌ Bad: Never store plain text
user.Password = password; // NEVER DO THIS
```

### Session Management

#### Session Configuration
- **Timeout**: 30 minutes of inactivity
- **HttpOnly**: Prevents JavaScript access (XSS protection)
- **Secure**: HTTPS-only cookies (production)
- **SameSite**: Strict mode (CSRF protection)

#### Session Security
```csharp
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;        // XSS protection
    options.Cookie.IsEssential = true;
    options.Cookie.Secure = true;          // HTTPS only (production)
    options.Cookie.SameSite = SameSiteMode.Strict; // CSRF protection
});
```

### Role-Based Access Control (RBAC)

#### User Roles
- **Student**: Can browse events, claim tickets, join carpools
- **Organizer**: Can create events, manage rooms, approve rentals
- **Admin**: Full system access, user/event approval

#### Authorization Checks
```csharp
// Check user role
if (user.Role != UserRole.Admin)
    return Forbid();

// Check resource ownership
if (event.OrganizerId != userId)
    return Forbid();
```

---

## Data Protection

### Encryption at Rest

#### Sensitive Data Fields
- Driver license numbers (AES-256)
- License plates (AES-256)
- Encryption keys stored in configuration

#### Encryption Implementation
```csharp
// Encrypt sensitive data
var encrypted = _encryptionService.Encrypt(licenseNumber);

// Decrypt when needed
var decrypted = _encryptionService.Decrypt(encrypted);
```

#### Key Management
- **Development**: Stored in appsettings.Development.json or User Secrets
- **Production**: Environment variables or Azure Key Vault
- **Key Rotation**: Rotate keys periodically
- **Key Storage**: Never commit keys to version control

### Encryption in Transit

#### HTTPS Configuration
- **Development**: Self-signed certificate (trusted locally)
- **Production**: Valid SSL certificate from CA
- **HSTS**: HTTP Strict Transport Security enabled
- **TLS Version**: TLS 1.2 or higher

#### Security Headers
```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'");
    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000");
    await next();
});
```

---

## Input Validation

### Validation Layers

#### 1. Client-Side Validation
- HTML5 form validation
- JavaScript validation
- Immediate user feedback

#### 2. Server-Side Validation
- Model validation attributes
- Custom validation in services
- Business rule validation

### Validation Examples

#### Email Validation
```csharp
if (!ValidationHelper.IsValidEmail(email))
    return (false, "Invalid email format", null);
```

#### String Validation
```csharp
if (!ValidationHelper.IsValidString(title, 
    minLength: Constants.Validation.MinEventTitleLength,
    maxLength: Constants.Validation.MaxEventTitleLength))
    return (false, "Invalid title length", null);
```

#### Sanitization
```csharp
var sanitized = ValidationHelper.SanitizeInput(userInput);
// Removes HTML tags and control characters
```

### SQL Injection Prevention

#### Parameterized Queries
Entity Framework Core uses parameterized queries automatically:

```csharp
// ✅ Good: EF Core parameterizes automatically
var events = await _context.Events
    .Where(e => e.Title.Contains(searchTerm))
    .ToListAsync();

// ❌ Bad: Never use string concatenation
var query = $"SELECT * FROM Events WHERE Title = '{searchTerm}'";
```

---

## Secure Communication

### QR Code Security

#### Token Signing
- **Algorithm**: HMAC-SHA256
- **Key**: Stored in configuration (environment variables in production)
- **Expiry**: 24 hours after event date
- **Verification**: Constant-time comparison

#### Token Structure
```json
{
  "payload": "base64_encoded_json",
  "signature": "base64_encoded_hmac"
}
```

#### Security Features
- Prevents ticket forgery
- Prevents guessing attacks
- Prevents replay attacks (expiry)
- Constant-time verification (timing attack prevention)

### API Security

#### Rate Limiting (Recommended)
```csharp
// Add rate limiting middleware
app.UseRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});
```

#### CORS Configuration
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins("https://trusted-domain.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

---

## Security Best Practices

### Code Security

#### 1. Never Trust User Input
```csharp
// ✅ Good: Validate and sanitize
var sanitized = ValidationHelper.SanitizeInput(userInput);
var validated = ValidationHelper.IsValidEmail(email);

// ❌ Bad: Trust user input
var query = userInput; // Dangerous!
```

#### 2. Use Secure Defaults
```csharp
// ✅ Good: Secure by default
options.Cookie.HttpOnly = true;
options.Cookie.Secure = true;

// ❌ Bad: Insecure defaults
options.Cookie.HttpOnly = false;
```

#### 3. Principle of Least Privilege
```csharp
// ✅ Good: Check specific permissions
if (user.Role != UserRole.Admin)
    return Forbid();

// ❌ Bad: Overly permissive
if (user != null) // Too permissive
    return Ok();
```

#### 4. Fail Securely
```csharp
// ✅ Good: Fail securely
try
{
    await ProcessPayment();
}
catch (Exception ex)
{
    LogError(ex);
    return (false, "Payment processing failed", null); // Don't expose details
}

// ❌ Bad: Expose sensitive information
catch (Exception ex)
{
    return (false, ex.ToString(), null); // Exposes stack trace
}
```

### Configuration Security

#### Environment Variables
```bash
# Production keys (never commit)
export SECURITY__TICKETSIGNINGKEY="production-key-here"
export SECURITY__ENCRYPTIONKEY="production-key-here"
```

#### User Secrets (Development)
```bash
dotnet user-secrets set "Security:TicketSigningKey" "dev-key"
dotnet user-secrets set "Security:EncryptionKey" "dev-key"
```

#### Azure Key Vault (Production)
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{vaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

---

## Vulnerability Management

### Common Vulnerabilities

#### 1. Cross-Site Scripting (XSS)

**Prevention**:
- HTML-encode all user input in views
- Use Razor's automatic encoding: `@userInput`
- Content Security Policy headers
- Input validation and sanitization

#### 2. Cross-Site Request Forgery (CSRF)

**Prevention**:
- Anti-forgery tokens on all POST requests
- SameSite cookie attribute
- Verify Referer header (optional)

#### 3. SQL Injection

**Prevention**:
- Use Entity Framework Core (parameterized queries)
- Never use string concatenation for SQL
- Validate all input

#### 4. Authentication Bypass

**Prevention**:
- Strong password requirements
- Session timeout
- Secure session storage
- Rate limiting on login

#### 5. Insecure Direct Object References

**Prevention**:
- Authorization checks on all resources
- Verify user owns resource
- Use indirect references where possible

### Security Headers

#### Recommended Headers
```csharp
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
Content-Security-Policy: default-src 'self'
Strict-Transport-Security: max-age=31536000; includeSubDomains
Referrer-Policy: strict-origin-when-cross-origin
Permissions-Policy: geolocation=(), microphone=(), camera=()
```

### Dependency Security

#### Regular Updates
```bash
# Check for outdated packages
dotnet list package --outdated

# Update packages
dotnet add package PackageName --version LatestVersion

# Check for vulnerabilities
dotnet list package --vulnerable
```

#### Security Advisories
- Monitor .NET security advisories
- Subscribe to security bulletins
- Review dependency vulnerabilities regularly

---

## Security Checklist

### Development
- [ ] All passwords hashed with BCrypt
- [ ] Input validation on all user inputs
- [ ] SQL injection prevention (EF Core)
- [ ] XSS prevention (HTML encoding)
- [ ] CSRF protection (anti-forgery tokens)
- [ ] Secure session configuration
- [ ] Sensitive data encrypted at rest
- [ ] HTTPS in production
- [ ] Security headers configured
- [ ] Error messages don't expose sensitive info

### Production
- [ ] Strong encryption keys (environment variables)
- [ ] Valid SSL certificate
- [ ] Security headers enabled
- [ ] Rate limiting configured
- [ ] Logging configured (no sensitive data)
- [ ] Database backups encrypted
- [ ] Access logs monitored
- [ ] Regular security updates
- [ ] Penetration testing completed
- [ ] Security incident response plan

---

## Incident Response

### Security Incident Procedure

1. **Identify**: Detect and confirm security incident
2. **Contain**: Isolate affected systems
3. **Eradicate**: Remove threat and vulnerabilities
4. **Recover**: Restore systems to normal operation
5. **Lessons Learned**: Document and improve

### Reporting

- Report security issues to: security@campusevents.com
- Include: Description, steps to reproduce, impact assessment
- Response time: Within 24 hours

---

## Compliance

### Data Privacy

- **PII Protection**: Personal information encrypted
- **Data Retention**: Policies for data retention
- **User Rights**: Data access and deletion rights
- **Privacy Policy**: Clear privacy policy

### Regulatory Compliance

- **GDPR**: European data protection (if applicable)
- **PIPEDA**: Canadian privacy law (if applicable)
- **FERPA**: Educational records (if applicable)

---

**Document Version**: 1.0
**Last Updated**: 2025-01-XX
**Maintained By**: Development Team

