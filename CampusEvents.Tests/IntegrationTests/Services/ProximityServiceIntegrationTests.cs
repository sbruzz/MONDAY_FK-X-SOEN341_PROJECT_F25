using Xunit;
using FluentAssertions;
using CampusEvents.Services;
using CampusEvents.Data;
using CampusEvents.Models;
using CampusEvents.Tests.TestHelpers;

namespace CampusEvents.Tests.IntegrationTests.Services;

/// <summary>
/// Integration tests for ProximityService using SQLite in-memory database
/// Tests end-to-end workflows with database interactions
/// </summary>
public class ProximityServiceIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ProximityService _service;

    public ProximityServiceIntegrationTests()
    {
        _context = IntegrationTestDbContextFactory.Create();
        _service = new ProximityService(_context);
    }

    public void Dispose()
    {
        IntegrationTestDbContextFactory.Dispose(_context);
    }

    [Fact]
    public async Task CheckDriverEligibility_WithDatabase_ShouldWorkEndToEnd()
    {
        // Setup
        var organizer = new User
        {
            Email = "organizer@test.com",
            PasswordHash = "hash",
            Name = "Test Organizer",
            Role = UserRole.Organizer,
            ApprovalStatus = ApprovalStatus.Approved
        };
        _context.Users.Add(organizer);
        await _context.SaveChangesAsync();

        var user = new User
        {
            Email = "student@test.com",
            PasswordHash = "hash",
            Name = "Test Student",
            Role = UserRole.Student,
            ApprovalStatus = ApprovalStatus.Approved
        };
        _context.Users.Add(user);

        var eventEntity = new Event
        {
            Title = "Test Event",
            Description = "Test",
            EventDate = DateTime.UtcNow.AddDays(7),
            Location = "Test Location",
            Capacity = 100,
            TicketType = TicketType.Free,
            Category = "Test",
            OrganizerId = organizer.Id,
            ApprovalStatus = ApprovalStatus.Approved
        };
        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        var driver = new Driver
        {
            UserId = user.Id,
            Capacity = 4,
            VehicleType = VehicleType.Sedan,
            DriverType = DriverType.Student,
            Status = DriverStatus.Active
        };
        _context.Drivers.Add(driver);
        await _context.SaveChangesAsync();

        // Call
        var result = await _service.CheckDriverEligibilityAsync(user.Id, eventEntity.Id);

        // Assertion
        result.Eligible.Should().BeTrue();
    }

    [Fact]
    public async Task CheckPassengerEligibility_WithNearbyOffers_ShouldReturnOffers()
    {
        // Setup
        var organizer = new User
        {
            Email = "organizer@test.com",
            PasswordHash = "hash",
            Name = "Test Organizer",
            Role = UserRole.Organizer,
            ApprovalStatus = ApprovalStatus.Approved
        };
        _context.Users.Add(organizer);
        await _context.SaveChangesAsync();

        var driverUser = new User
        {
            Email = "driver@test.com",
            PasswordHash = "hash",
            Name = "Driver",
            Role = UserRole.Student,
            ApprovalStatus = ApprovalStatus.Approved
        };
        var passengerUser = new User
        {
            Email = "passenger@test.com",
            PasswordHash = "hash",
            Name = "Passenger",
            Role = UserRole.Student,
            ApprovalStatus = ApprovalStatus.Approved
        };
        _context.Users.AddRange(driverUser, passengerUser);

        var eventEntity = new Event
        {
            Title = "Test Event",
            Description = "Test",
            EventDate = DateTime.UtcNow.AddDays(7),
            Location = "Test Location",
            Capacity = 100,
            TicketType = TicketType.Free,
            Category = "Test",
            OrganizerId = organizer.Id,
            ApprovalStatus = ApprovalStatus.Approved
        };
        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        var driver = new Driver
        {
            UserId = driverUser.Id,
            Capacity = 4,
            VehicleType = VehicleType.Sedan,
            DriverType = DriverType.Student,
            Status = DriverStatus.Active
        };
        _context.Drivers.Add(driver);
        await _context.SaveChangesAsync();

        // Create offer with coordinates
        var offer = new CarpoolOffer
        {
            DriverId = driver.Id,
            EventId = eventEntity.Id,
            SeatsAvailable = 3,
            DepartureInfo = "Metro Station",
            DepartureTime = DateTime.UtcNow.AddDays(7).AddHours(-1),
            Status = CarpoolOfferStatus.Active,
            Latitude = 45.4972, // Concordia coordinates
            Longitude = -73.5789
        };
        _context.CarpoolOffers.Add(offer);
        await _context.SaveChangesAsync();

        // Call: Check eligibility with nearby coordinates
        var result = await _service.CheckPassengerEligibilityAsync(
            passengerUser.Id,
            eventEntity.Id,
            userLatitude: 45.4974, // Very close to offer
            userLongitude: -73.5790,
            proximityThresholdKm: 10.0
        );

        // Assertion
        result.Eligible.Should().BeTrue();
        result.NearbyOffers.Should().NotBeNull();
        result.NearbyOffers!.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetNearbyOffers_WithDistance_ShouldSortByDistance()
    {
        // Setup
        var organizer = new User
        {
            Email = "organizer@test.com",
            PasswordHash = "hash",
            Name = "Test Organizer",
            Role = UserRole.Organizer,
            ApprovalStatus = ApprovalStatus.Approved
        };
        _context.Users.Add(organizer);
        await _context.SaveChangesAsync();

        var driverUser1 = new User
        {
            Email = "driver1@test.com",
            PasswordHash = "hash",
            Name = "Driver 1",
            Role = UserRole.Student,
            ApprovalStatus = ApprovalStatus.Approved
        };
        var driverUser2 = new User
        {
            Email = "driver2@test.com",
            PasswordHash = "hash",
            Name = "Driver 2",
            Role = UserRole.Student,
            ApprovalStatus = ApprovalStatus.Approved
        };
        _context.Users.AddRange(driverUser1, driverUser2);

        var eventEntity = new Event
        {
            Title = "Test Event",
            Description = "Test",
            EventDate = DateTime.UtcNow.AddDays(7),
            Location = "Test Location",
            Capacity = 100,
            TicketType = TicketType.Free,
            Category = "Test",
            OrganizerId = organizer.Id,
            ApprovalStatus = ApprovalStatus.Approved
        };
        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        var driver1 = new Driver
        {
            UserId = driverUser1.Id,
            Capacity = 4,
            VehicleType = VehicleType.Sedan,
            DriverType = DriverType.Student,
            Status = DriverStatus.Active
        };
        var driver2 = new Driver
        {
            UserId = driverUser2.Id,
            Capacity = 4,
            VehicleType = VehicleType.Sedan,
            DriverType = DriverType.Student,
            Status = DriverStatus.Active
        };
        _context.Drivers.AddRange(driver1, driver2);
        await _context.SaveChangesAsync();

        // Create offers at different distances
        var offer1 = new CarpoolOffer
        {
            DriverId = driver1.Id,
            EventId = eventEntity.Id,
            SeatsAvailable = 3,
            DepartureInfo = "Nearby",
            DepartureTime = DateTime.UtcNow.AddDays(7).AddHours(-1),
            Status = CarpoolOfferStatus.Active,
            Latitude = 45.4972, // Close
            Longitude = -73.5789
        };
        var offer2 = new CarpoolOffer
        {
            DriverId = driver2.Id,
            EventId = eventEntity.Id,
            SeatsAvailable = 3,
            DepartureInfo = "Far",
            DepartureTime = DateTime.UtcNow.AddDays(7).AddHours(-1),
            Status = CarpoolOfferStatus.Active,
            Latitude = 45.5048, // Farther
            Longitude = -73.5772
        };
        _context.CarpoolOffers.AddRange(offer1, offer2);
        await _context.SaveChangesAsync();

        // Call: Get nearby offers from a specific location
        var userLat = 45.4970;
        var userLon = -73.5790;
        var result = await _service.GetNearbyOffersAsync(eventEntity.Id, userLat, userLon, maxDistanceKm: 10.0);

        // Assertion: Should be sorted by distance (closest first)
        result.Should().NotBeEmpty();
        if (result.Count >= 2)
        {
            result[0].Distance.Should().BeLessThan(result[1].Distance ?? double.MaxValue);
        }
    }
}

