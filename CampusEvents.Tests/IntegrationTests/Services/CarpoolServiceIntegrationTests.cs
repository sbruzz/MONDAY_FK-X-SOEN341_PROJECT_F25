using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Services;
using CampusEvents.Data;
using CampusEvents.Models;
using CampusEvents.Tests.TestHelpers;

namespace CampusEvents.Tests.IntegrationTests.Services;

/// <summary>
/// Integration tests for CarpoolService using SQLite in-memory database
/// Tests end-to-end workflows and database interactions
/// </summary>
public class CarpoolServiceIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly CarpoolService _service;

    public CarpoolServiceIntegrationTests()
    {
        _context = IntegrationTestDbContextFactory.Create();
        _service = new CarpoolService(_context);
    }

    public void Dispose()
    {
        IntegrationTestDbContextFactory.Dispose(_context);
    }

    [Fact]
    public async Task RegisterDriverAsync_EndToEnd_ShouldPersistInDatabase()
    {
        // Setup
        var user = new User
        {
            Email = "student@test.com",
            PasswordHash = "hash",
            Name = "Test Student",
            Role = UserRole.Student,
            ApprovalStatus = ApprovalStatus.Approved
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Call
        var result = await _service.RegisterDriverAsync(user.Id, 4, VehicleType.Sedan, DriverType.Student);

        // Assertion
        result.Success.Should().BeTrue();
        
        // Verify persistence
        var savedDriver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == user.Id);
        savedDriver.Should().NotBeNull();
        savedDriver!.Capacity.Should().Be(4);
        savedDriver.VehicleType.Should().Be(VehicleType.Sedan);
    }

    [Fact]
    public async Task CreateOfferAndJoin_EndToEnd_ShouldUpdateSeatsCorrectly()
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

        // Register driver and activate
        var registerResult = await _service.RegisterDriverAsync(driverUser.Id, 4, VehicleType.Sedan, DriverType.Student);
        var driver = await _context.Drivers.FindAsync(registerResult.Driver!.Id);
        driver!.Status = DriverStatus.Active;
        await _context.SaveChangesAsync();

        // Create offer
        var offerResult = await _service.CreateOfferAsync(driver.Id, eventEntity.Id, "Metro", DateTime.UtcNow.AddDays(7).AddHours(-1));
        offerResult.Success.Should().BeTrue();

        var initialOffer = await _context.CarpoolOffers.FindAsync(offerResult.Offer!.Id);
        initialOffer!.SeatsAvailable.Should().Be(4);

        // Join offer
        var joinResult = await _service.JoinOfferAsync(offerResult.Offer.Id, passengerUser.Id);
        joinResult.Success.Should().BeTrue();

        // Verify seats updated
        var updatedOffer = await _context.CarpoolOffers.FindAsync(offerResult.Offer.Id);
        updatedOffer!.SeatsAvailable.Should().Be(3);

        // Verify passenger record created
        var passenger = await _context.CarpoolPassengers
            .FirstOrDefaultAsync(cp => cp.OfferId == offerResult.Offer.Id && cp.PassengerId == passengerUser.Id);
        passenger.Should().NotBeNull();
        passenger!.Status.Should().Be(PassengerStatus.Confirmed);
    }

    [Fact]
    public async Task ApproveDriver_EndToEnd_ShouldAllowCreatingOffers()
    {
        // Setup
        var user = new User
        {
            Email = "student@test.com",
            PasswordHash = "hash",
            Name = "Test Student",
            Role = UserRole.Student,
            ApprovalStatus = ApprovalStatus.Approved
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var registerResult = await _service.RegisterDriverAsync(user.Id, 4, VehicleType.Sedan, DriverType.Student);
        var driverId = registerResult.Driver!.Id;

        // Approve driver
        var approveResult = await _service.ApproveDriverAsync(driverId);
        approveResult.Success.Should().BeTrue();

        var driver = await _context.Drivers.FindAsync(driverId);
        driver!.Status.Should().Be(DriverStatus.Active);

        // Now should be able to create offer
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

        var offerResult = await _service.CreateOfferAsync(driverId, eventEntity.Id, "Metro", DateTime.UtcNow.AddDays(7).AddHours(-1));
        offerResult.Success.Should().BeTrue();
    }
}

