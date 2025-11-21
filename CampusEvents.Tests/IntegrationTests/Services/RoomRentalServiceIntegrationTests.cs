using Xunit;
using FluentAssertions;
using CampusEvents.Services;
using CampusEvents.Data;
using CampusEvents.Models;
using CampusEvents.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace CampusEvents.Tests.IntegrationTests.Services;

/// <summary>
/// Integration tests for RoomRentalService using SQLite in-memory database
/// Tests end-to-end workflows and database interactions
/// </summary>
public class RoomRentalServiceIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly RoomRentalService _service;

    public RoomRentalServiceIntegrationTests()
    {
        _context = IntegrationTestDbContextFactory.Create();
        _service = new RoomRentalService(_context);
    }

    public void Dispose()
    {
        IntegrationTestDbContextFactory.Dispose(_context);
    }

    [Fact]
    public async Task CreateRoomAndRequestRental_EndToEnd_ShouldPersistCorrectly()
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
        var renter = new User
        {
            Email = "renter@test.com",
            PasswordHash = "hash",
            Name = "Test Renter",
            Role = UserRole.Student,
            ApprovalStatus = ApprovalStatus.Approved
        };
        _context.Users.AddRange(organizer, renter);
        await _context.SaveChangesAsync();

        // Create room
        var roomResult = await _service.CreateRoomAsync(
            organizer.Id,
            "Test Room",
            "123 Test St",
            20,
            hourlyRate: 25.00m
        );
        roomResult.Success.Should().BeTrue();

        var room = await _context.Rooms.FindAsync(roomResult.Room!.Id);
        room.Should().NotBeNull();

        // Request rental
        var startTime = DateTime.UtcNow.AddDays(1).AddHours(10);
        var endTime = DateTime.UtcNow.AddDays(1).AddHours(12);
        var rentalResult = await _service.RequestRentalAsync(
            room.Id,
            renter.Id,
            startTime,
            endTime,
            purpose: "Study session"
        );

        rentalResult.Success.Should().BeTrue();
        rentalResult.Rental.Should().NotBeNull();
        if (rentalResult.Rental != null)
        {
            rentalResult.Rental.TotalCost.Should().Be(50.00m); // 2 hours * $25/hour
        }

        // Verify persistence
        var savedRental = await _context.RoomRentals
            .Include(rr => rr.Room)
            .Include(rr => rr.Renter)
            .FirstOrDefaultAsync(rr => rr.Id == rentalResult.Rental.Id);

        savedRental.Should().NotBeNull();
        savedRental!.Status.Should().Be(RentalStatus.Pending);
        savedRental.Room.Id.Should().Be(room.Id);
        savedRental.Renter.Id.Should().Be(renter.Id);
    }

    [Fact]
    public async Task ApproveRental_EndToEnd_ShouldUpdateStatusAndPreventDoubleBooking()
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
        var renter1 = new User
        {
            Email = "renter1@test.com",
            PasswordHash = "hash",
            Name = "Test Renter 1",
            Role = UserRole.Student,
            ApprovalStatus = ApprovalStatus.Approved
        };
        var renter2 = new User
        {
            Email = "renter2@test.com",
            PasswordHash = "hash",
            Name = "Test Renter 2",
            Role = UserRole.Student,
            ApprovalStatus = ApprovalStatus.Approved
        };
        _context.Users.AddRange(organizer, renter1, renter2);
        await _context.SaveChangesAsync();

        var roomResult = await _service.CreateRoomAsync(organizer.Id, "Test Room", "123 Test St", 20);
        var roomId = roomResult.Room!.Id;

        var startTime = DateTime.UtcNow.AddDays(1).AddHours(10);
        var endTime = DateTime.UtcNow.AddDays(1).AddHours(12);

        // First rental
        var rental1Result = await _service.RequestRentalAsync(roomId, renter1.Id, startTime, endTime);
        rental1Result.Success.Should().BeTrue();

        // Approve first rental
        var approveResult = await _service.ApproveRentalAsync(rental1Result.Rental!.Id, organizer.Id);
        approveResult.Success.Should().BeTrue();

        var approvedRental = await _context.RoomRentals.FindAsync(rental1Result.Rental.Id);
        approvedRental!.Status.Should().Be(RentalStatus.Approved);

        // Try to book overlapping time - should fail
        var rental2Result = await _service.RequestRentalAsync(
            roomId,
            renter2.Id,
            startTime.AddMinutes(30), // Overlaps
            endTime.AddMinutes(30)
        );

        rental2Result.Success.Should().BeFalse();
        rental2Result.Message.Should().Contain("already booked");
    }

    [Fact]
    public async Task GetAvailableRooms_EndToEnd_ShouldFilterCorrectly()
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
        var renter = new User
        {
            Email = "renter@test.com",
            PasswordHash = "hash",
            Name = "Test Renter",
            Role = UserRole.Student,
            ApprovalStatus = ApprovalStatus.Approved
        };
        _context.Users.AddRange(organizer, renter);
        await _context.SaveChangesAsync();

        // Create two rooms
        var room1Result = await _service.CreateRoomAsync(organizer.Id, "Room 1", "123 Test St", 20);
        var room2Result = await _service.CreateRoomAsync(organizer.Id, "Room 2", "456 Test St", 30);

        var startTime = DateTime.UtcNow.AddDays(1).AddHours(10);
        var endTime = DateTime.UtcNow.AddDays(1).AddHours(12);

        // Book room 1
        var rentalResult = await _service.RequestRentalAsync(room1Result.Room!.Id, renter.Id, startTime, endTime);
        await _service.ApproveRentalAsync(rentalResult.Rental!.Id, organizer.Id);

        // Check availability for same time slot
        var availableRooms = await _service.GetAvailableRoomsAsync(startTime, endTime);

        // Assertion
        availableRooms.Should().Contain(r => r.Id == room2Result.Room!.Id);
        availableRooms.Should().NotContain(r => r.Id == room1Result.Room.Id);
    }
}

