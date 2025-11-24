using CampusEvents.Data;
using Microsoft.EntityFrameworkCore;

namespace CampusEvents.Services;

/// <summary>
/// Extension methods for service registration and dependency injection
/// Provides convenient methods for configuring services
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Adds all Campus Events services to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for method chaining</returns>
    public static IServiceCollection AddCampusEventsServices(this IServiceCollection services)
    {
        // Register core services
        services.AddScoped<CarpoolService>();
        services.AddScoped<RoomRentalService>();
        services.AddScoped<ProximityService>();
        
        // Register data services
        services.AddTransient<DbCSVCommunicator>();
        
        return services;
    }

    /// <summary>
    /// Configures the database context with SQLite
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="connectionString">Database connection string</param>
    /// <returns>Service collection for method chaining</returns>
    public static IServiceCollection AddCampusEventsDbContext(
        this IServiceCollection services,
        string? connectionString = null)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite(connectionString ?? "Data Source=campusevents.db");
            
            // Enable sensitive data logging in development
            #if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            #endif
        });

        return services;
    }

    /// <summary>
    /// Configures session management for the application
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="timeoutMinutes">Session timeout in minutes</param>
    /// <returns>Service collection for method chaining</returns>
    public static IServiceCollection AddCampusEventsSession(
        this IServiceCollection services,
        int timeoutMinutes = Constants.Session.DefaultTimeoutMinutes)
    {
        services.AddDistributedMemoryCache();
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(timeoutMinutes);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
        });

        return services;
    }
}

