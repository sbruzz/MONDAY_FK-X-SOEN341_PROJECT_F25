using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Services;

/// <summary>
/// Application entry point and service configuration for Campus Events system.
/// This file configures dependency injection, middleware pipeline, and application startup.
/// </summary>
/// <remarks>
/// The application uses ASP.NET Core 9.0 with Razor Pages architecture.
/// Services are registered with appropriate lifetimes (Singleton, Scoped, Transient).
/// </remarks>
var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// SERVICE REGISTRATION
// ============================================================================

/// <summary>
/// Register Razor Pages service for page-based MVC architecture.
/// Razor Pages provides a simpler model for page-focused scenarios compared to traditional MVC.
/// </summary>
builder.Services.AddRazorPages();

// ============================================================================
// SESSION CONFIGURATION
// ============================================================================

/// <summary>
/// Configure distributed memory cache for session storage.
/// In production, consider using Redis or SQL Server for distributed scenarios.
/// </summary>
builder.Services.AddDistributedMemoryCache();

/// <summary>
/// Configure session middleware with security settings.
/// Sessions are used for user authentication and state management.
/// </summary>
/// <remarks>
/// Session configuration:
/// - IdleTimeout: 30 minutes of inactivity before session expires
/// - HttpOnly: Prevents JavaScript access to session cookie (XSS protection)
/// - IsEssential: Required for authentication functionality
/// </remarks>
builder.Services.AddSession(options =>
{
    // Session expires after 30 minutes of inactivity
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    
    // HttpOnly cookie prevents JavaScript access (XSS protection)
    options.Cookie.HttpOnly = true;
    
    // Essential cookie required for authentication
    options.Cookie.IsEssential = true;
    
    // Note: In production, set options.Cookie.Secure = true for HTTPS-only cookies
});

// ============================================================================
// DATABASE CONFIGURATION
// ============================================================================

/// <summary>
/// Register Entity Framework Core DbContext with SQLite provider.
/// The connection string is read from appsettings.json or defaults to local SQLite file.
/// </summary>
/// <remarks>
/// Connection string format: "Data Source=campusevents.db"
/// For production, consider migrating to SQL Server or PostgreSQL.
/// </remarks>
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=campusevents.db"
    )
);

// ============================================================================
// APPLICATION SERVICES REGISTRATION
// ============================================================================

/// <summary>
/// Register CSV data communicator as transient service.
/// Transient lifetime: New instance created for each service request.
/// Used for CSV import/export operations.
/// </summary>
builder.Services.AddTransient<DbCSVCommunicator>();

// ============================================================================
// SECURITY SERVICES (SINGLETON)
// ============================================================================

/// <summary>
/// Register ticket signing service as singleton.
/// Singleton lifetime: Single instance shared across all requests.
/// Used for generating and validating HMAC-signed QR code tokens.
/// </summary>
/// <remarks>
/// Singleton is appropriate because:
/// - Service is stateless (no per-request state)
/// - Thread-safe (HMAC operations are thread-safe)
/// - Performance: Avoids recreating service for each request
/// </remarks>
builder.Services.AddSingleton<TicketSigningService>();

/// <summary>
/// Register encryption service as singleton.
/// Provides AES-256 encryption for sensitive data (driver licenses, license plates).
/// </summary>
/// <remarks>
/// Singleton is appropriate because:
/// - Encryption key is loaded once at startup
/// - Service is stateless and thread-safe
/// - Performance optimization for cryptographic operations
/// </remarks>
builder.Services.AddSingleton<EncryptionService>();

/// <summary>
/// Register license validation service as singleton.
/// Validates driver license numbers and formats by province.
/// </summary>
builder.Services.AddSingleton<LicenseValidationService>();

// ============================================================================
// BUSINESS LOGIC SERVICES (SCOPED)
// ============================================================================

/// <summary>
/// Register carpool service as scoped service.
/// Scoped lifetime: One instance per HTTP request.
/// Manages driver registration, carpool offers, and passenger assignments.
/// </summary>
/// <remarks>
/// Scoped is appropriate because:
/// - Service uses DbContext (which is scoped)
/// - Service may have per-request state
/// - Ensures proper disposal of resources
/// </remarks>
builder.Services.AddScoped<CarpoolService>();

/// <summary>
/// Register room rental service as scoped service.
/// Manages room creation, rental requests, and availability checking.
/// </summary>
builder.Services.AddScoped<RoomRentalService>();

/// <summary>
/// Register proximity service as scoped service.
/// Calculates geographic proximity for carpool offers.
/// </summary>
builder.Services.AddScoped<ProximityService>();

/// <summary>
/// Register notification service as scoped service.
/// Manages user notifications for various system events.
/// </summary>
/// <remarks>
/// NotificationService is used by other services (CarpoolService, RoomRentalService)
/// to send notifications after state changes.
/// </remarks>
builder.Services.AddScoped<NotificationService>();

// ============================================================================
// APPLICATION BUILD AND CONFIGURATION
// ============================================================================

/// <summary>
/// Build the web application instance.
/// This compiles the service provider and configures the application pipeline.
/// </summary>
var app = builder.Build();

// ============================================================================
// DATABASE INITIALIZATION
// ============================================================================

/// <summary>
/// Initialize database and test CSV communication on application startup.
/// This ensures the database is accessible and CSV operations work correctly.
/// </summary>
/// <remarks>
/// Using a scoped service provider to get DbCSVCommunicator instance.
/// This runs once at application startup, not on every request.
/// </remarks>
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbCSV = services.GetRequiredService<DbCSVCommunicator>();
    
    // Test CSV communication functionality
    dbCSV.Test();
}

// ============================================================================
// HTTP REQUEST PIPELINE CONFIGURATION
// ============================================================================

/// <summary>
/// Configure exception handling for production environment.
/// In development, detailed error pages are shown by default.
/// In production, redirect to error page for security.
/// </summary>
if (!app.Environment.IsDevelopment())
{
    // Redirect exceptions to error page
    app.UseExceptionHandler("/Error");
    
    // Enable HTTP Strict Transport Security (HSTS)
    // HSTS forces browsers to use HTTPS for future requests
    // Default value is 30 days - adjust for production scenarios
    // See: https://aka.ms/aspnetcore-hsts
    app.UseHsts();
}

// ============================================================================
// MIDDLEWARE PIPELINE
// ============================================================================

/// <summary>
/// Enable HTTPS redirection middleware.
/// Redirects HTTP requests to HTTPS for security.
/// </summary>
/// <remarks>
/// In production, ensure SSL certificates are properly configured.
/// </remarks>
app.UseHttpsRedirection();

/// <summary>
/// Enable routing middleware.
/// Matches incoming requests to route handlers (Razor Pages).
/// </summary>
app.UseRouting();

/// <summary>
/// Enable session middleware.
/// Must be called after UseRouting() and before UseAuthorization().
/// Provides access to HttpContext.Session for storing user state.
/// </summary>
app.UseSession();

/// <summary>
/// Enable authorization middleware.
/// Enforces authorization policies and role-based access control.
/// Must be called after UseRouting() and UseSession().
/// </summary>
app.UseAuthorization();

// ============================================================================
// ENDPOINT MAPPING
// ============================================================================

/// <summary>
/// Map static assets (CSS, JavaScript, images) from wwwroot directory.
/// Static files are served directly without routing.
/// </summary>
app.MapStaticAssets();

/// <summary>
/// Map Razor Pages endpoints.
/// Razor Pages are discovered automatically from the Pages/ directory.
/// WithStaticAssets() enables static asset mapping for Razor Pages.
/// </summary>
app.MapRazorPages()
   .WithStaticAssets();

// ============================================================================
// APPLICATION STARTUP
// ============================================================================

/// <summary>
/// Start the web application and begin listening for HTTP requests.
/// This is a blocking call that runs until the application is shut down.
/// </summary>
/// <remarks>
/// The application listens on:
/// - HTTP: http://localhost:5136 (default)
/// - HTTPS: https://localhost:7295 (default, requires certificate)
/// 
/// Ports can be configured in Properties/launchSettings.json or via command line:
/// - dotnet run --urls "http://localhost:5000"
/// </remarks>
app.Run();