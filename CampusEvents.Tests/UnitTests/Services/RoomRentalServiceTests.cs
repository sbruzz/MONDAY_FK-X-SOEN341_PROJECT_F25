using Xunit;
using FluentAssertions;
using CampusEvents.Services;
using CampusEvents.Data;
using CampusEvents.Models;
using CampusEvents.Tests.TestHelpers;

namespace CampusEvents.Tests.UnitTests.Services;

/// <summary>
/// Unit tests for RoomRentalService using equivalence partitioning and boundary testing
/// </summary>
public class RoomRentalServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly RoomRentalService _service;

    public RoomRentalServiceTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _service = new RoomRentalService(_context);
    }

    public void Dispose()
    {
        InMemoryDbContextFactory.Dispose(_context);
    }

    #region CreateRoomAsync Tests

    [Fact]
    public async Task CreateRoomAsync_ValidInputs_ReturnsSuccess()
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

        // Call
        var result = await _service.CreateRoomAsync(
            organizerId: organizer.Id,
            name: "Test Room",
            address: "123 Test St",
            capacity: 20
        );

        // Assertion
        result.Success.Should().BeTrue();
        result.Room.Should().NotBeNull();
        result.Room!.Name.Should().Be("Test Room");
        result.Room.Capacity.Should().Be(20);
        result.Room.Status.Should().Be(RoomStatus.Enabled);
    }

    [Fact]
    public async Task CreateRoomAsync_NonExistentUser_ReturnsFailure()
    {
        // Call
        var result = await _service.CreateRoomAsync(
            organizerId: 999,
            name: "Test Room",
            address: "123 Test St",
            capacity: 20
        );

        // Assertion
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task CreateRoomAsync_NonOrganizerUser_ReturnsFailure()
    {
        // Setup: Student trying to create room
        var student = new User
        {
            Email = "student@test.com",
            PasswordHash = "hash",
            Name = "Test Student",
            Role = UserRole.Student,
            ApprovalStatus = ApprovalStatus.Approved
        };
        _context.Users.Add(student);
        await _context.SaveChangesAsync();

        // Call
        var result = await _service.CreateRoomAsync(
            organizerId: student.Id,
            name: "Test Room",
            address: "123 Test St",
            capacity: 20
        );

        // Assertion
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Only organizers can create rooms");
    }

    [Theory]
    [InlineData(0)]   // Boundary: below valid range
    [InlineData(-1)]  // Invalid: negative
    public async Task CreateRoomAsync_InvalidCapacityBoundary_ReturnsFailure(int capacity)
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

        // Call
        var result = await _service.CreateRoomAsync(
            organizerId: organizer.Id,
            name: "Test Room",
            address: "123 Test St",
            capacity: capacity
        );

        // Assertion
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("at least 1");
    }

    [Theory]
    [InlineData(1)]   // Boundary: minimum valid
    [InlineData(100)] // Large capacity
    public async Task CreateRoomAsync_ValidCapacityBoundary_ReturnsSuccess(int capacity)
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

        // Call
        var result = await _service.CreateRoomAsync(
            organizerId: organizer.Id,
            name: "Test Room",
            address: "123 Test St",
            capacity: capacity
        );

        // Assertion
        result.Success.Should().BeTrue();
        result.Room!.Capacity.Should().Be(capacity);
    }

    [Fact]
    public async Task CreateRoomAsync_InvalidAvailabilityWindow_ReturnsFailure()
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

        var startTime = DateTime.UtcNow.AddDays(10);
        var endTime = DateTime.UtcNow.AddDays(5); // End before start

        // Call
        var result = await _service.CreateRoomAsync(
            organizerId: organizer.Id,
            name: "Test Room",
            address: "123 Test St",
            capacity: 20,
            availabilityStart: startTime,
            availabilityEnd: endTime
        );

        // Assertion
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("after start time");
    }

    #endregion

    #region RequestRentalAsync Tests

    [Fact]
    public async Task RequestRentalAsync_ValidRental_ReturnsSuccess()
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

        var roomResult = await _service.CreateRoomAsync(organizer.Id, "Test Room", "123 Test St", 20);
        var roomId = roomResult.Room!.Id;

        var startTime = DateTime.UtcNow.AddDays(1).AddHours(10);
        var endTime = DateTime.UtcNow.AddDays(1).AddHours(12);

        // Call
        var result = await _service.RequestRentalAsync(
            roomId: roomId,
            renterId: renter.Id,
            startTime: startTime,
            endTime: endTime,
            purpose: "Study session"
        );

        // Assertion
        result.Success.Should().BeTrue();
        result.Rental.Should().NotBeNull();
        result.Rental!.Status.Should().Be(RentalStatus.Pending);
    }

    [Fact]
    public async Task RequestRentalAsync_DisabledRoom_ReturnsFailure()
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

        var roomResult = await _service.CreateRoomAsync(organizer.Id, "Test Room", "123 Test St", 20);
        var room = await _context.Rooms.FindAsync(roomResult.Room!.Id);
        room!.Status = RoomStatus.Disabled;
        await _context.SaveChangesAsync();

        var startTime = DateTime.UtcNow.AddDays(1).AddHours(10);
        var endTime = DateTime.UtcNow.AddDays(1).AddHours(12);

        // Call
        var result = await _service.RequestRentalAsync(room.Id, renter.Id, startTime, endTime);

        // Assertion
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("disabled");
    }

    [Fact]
    public async Task RequestRentalAsync_PastTime_ReturnsFailure()
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

        var roomResult = await _service.CreateRoomAsync(organizer.Id, "Test Room", "123 Test St", 20);
        var roomId = roomResult.Room!.Id;

        var startTime = DateTime.UtcNow.AddDays(-1); // Past time
        var endTime = DateTime.UtcNow.AddDays(-1).AddHours(2);

        // Call
        var result = await _service.RequestRentalAsync(roomId, renter.Id, startTime, endTime);

        // Assertion
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("past");
    }

    [Fact]
    public async Task RequestRentalAsync_EndTimeBeforeStartTime_ReturnsFailure()
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

        var roomResult = await _service.CreateRoomAsync(organizer.Id, "Test Room", "123 Test St", 20);
        var roomId = roomResult.Room!.Id;

        var startTime = DateTime.UtcNow.AddDays(1).AddHours(12);
        var endTime = DateTime.UtcNow.AddDays(1).AddHours(10); // End before start

        // Call
        var result = await _service.RequestRentalAsync(roomId, renter.Id, startTime, endTime);

        // Assertion
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("after start time");
    }

    [Fact]
    public async Task RequestRentalAsync_ExceedsCapacity_ReturnsFailure()
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

        var roomResult = await _service.CreateRoomAsync(organizer.Id, "Test Room", "123 Test St", 20);
        var roomId = roomResult.Room!.Id;

        var startTime = DateTime.UtcNow.AddDays(1).AddHours(10);
        var endTime = DateTime.UtcNow.AddDays(1).AddHours(12);

        // Call: Request with more attendees than capacity
        var result = await _service.RequestRentalAsync(
            roomId: roomId,
            renterId: renter.Id,
            startTime: startTime,
            endTime: endTime,
            expectedAttendees: 25 // Exceeds capacity of 20
        );

        // Assertion
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("exceeds room capacity");
    }

    [Fact]
    public async Task RequestRentalAsync_DoubleBooking_ReturnsFailure()
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
        var firstRental = await _service.RequestRentalAsync(roomId, renter1.Id, startTime, endTime);
        firstRental.Success.Should().BeTrue();

        // Approve first rental
        await _service.ApproveRentalAsync(firstRental.Rental!.Id, organizer.Id);

        // Call: Try to book overlapping time
        var result = await _service.RequestRentalAsync(
            roomId: roomId,
            renterId: renter2.Id,
            startTime: startTime.AddMinutes(30), // Overlaps
            endTime: endTime.AddMinutes(30)
        );

        // Assertion
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already booked");
    }

    [Fact]
    public async Task RequestRentalAsync_OutsideAvailabilityWindow_ReturnsFailure()
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

        var availabilityStart = DateTime.UtcNow.AddDays(5);
        var availabilityEnd = DateTime.UtcNow.AddDays(10);
        var roomResult = await _service.CreateRoomAsync(
            organizer.Id,
            "Test Room",
            "123 Test St",
            20,
            availabilityStart: availabilityStart,
            availabilityEnd: availabilityEnd
        );
        var roomId = roomResult.Room!.Id;

        // Call: Request outside availability window
        var startTime = DateTime.UtcNow.AddDays(3); // Before availability start
        var endTime = DateTime.UtcNow.AddDays(3).AddHours(2);

        var result = await _service.RequestRentalAsync(roomId, renter.Id, startTime, endTime);

        // Assertion
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not available");
    }

    #endregion

    #region ApproveRentalAsync Tests

    [Fact]
    public async Task ApproveRentalAsync_ValidApproval_ReturnsSuccess()
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

        var roomResult = await _service.CreateRoomAsync(organizer.Id, "Test Room", "123 Test St", 20);
        var roomId = roomResult.Room!.Id;

        var startTime = DateTime.UtcNow.AddDays(1).AddHours(10);
        var endTime = DateTime.UtcNow.AddDays(1).AddHours(12);
        var rentalResult = await _service.RequestRentalAsync(roomId, renter.Id, startTime, endTime);
        var rentalId = rentalResult.Rental!.Id;

        // Call
        var result = await _service.ApproveRentalAsync(rentalId, organizer.Id);

        // Assertion
        result.Success.Should().BeTrue();
        var rental = await _context.RoomRentals.FindAsync(rentalId);
        rental!.Status.Should().Be(RentalStatus.Approved);
    }

    [Fact]
    public async Task ApproveRentalAsync_NonOrganizer_ReturnsFailure()
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
        var otherUser = new User
        {
            Email = "other@test.com",
            PasswordHash = "hash",
            Name = "Other User",
            Role = UserRole.Student,
            ApprovalStatus = ApprovalStatus.Approved
        };
        _context.Users.AddRange(organizer, renter, otherUser);
        await _context.SaveChangesAsync();

        var roomResult = await _service.CreateRoomAsync(organizer.Id, "Test Room", "123 Test St", 20);
        var roomId = roomResult.Room!.Id;

        var startTime = DateTime.UtcNow.AddDays(1).AddHours(10);
        var endTime = DateTime.UtcNow.AddDays(1).AddHours(12);
        var rentalResult = await _service.RequestRentalAsync(roomId, renter.Id, startTime, endTime);
        var rentalId = rentalResult.Rental!.Id;

        // Call: Non-organizer tries to approve
        var result = await _service.ApproveRentalAsync(rentalId, otherUser.Id);

        // Assertion
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Only the room organizer");
    }

    #endregion

    #region RejectRentalAsync Tests

    [Fact]
    public async Task RejectRentalAsync_ValidRejection_ReturnsSuccess()
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

        var roomResult = await _service.CreateRoomAsync(organizer.Id, "Test Room", "123 Test St", 20);
        var roomId = roomResult.Room!.Id;

        var startTime = DateTime.UtcNow.AddDays(1).AddHours(10);
        var endTime = DateTime.UtcNow.AddDays(1).AddHours(12);
        var rentalResult = await _service.RequestRentalAsync(roomId, renter.Id, startTime, endTime);
        var rentalId = rentalResult.Rental!.Id;

        // Call
        var result = await _service.RejectRentalAsync(rentalId, organizer.Id, "Not available");

        // Assertion
        result.Success.Should().BeTrue();
        var rental = await _context.RoomRentals.FindAsync(rentalId);
        rental!.Status.Should().Be(RentalStatus.Rejected);
        rental.AdminNotes.Should().Be("Not available");
    }

    #endregion

    #region GetAvailableRoomsAsync Tests

    [Fact]
    public async Task GetAvailableRoomsAsync_NoOverlaps_ReturnsAvailableRooms()
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

        var roomResult = await _service.CreateRoomAsync(organizer.Id, "Test Room", "123 Test St", 20);
        var roomId = roomResult.Room!.Id;

        var startTime = DateTime.UtcNow.AddDays(1).AddHours(10);
        var endTime = DateTime.UtcNow.AddDays(1).AddHours(12);

        // Call: Check availability for different time slot
        var availableStart = DateTime.UtcNow.AddDays(1).AddHours(14);
        var availableEnd = DateTime.UtcNow.AddDays(1).AddHours(16);
        var result = await _service.GetAvailableRoomsAsync(availableStart, availableEnd);

        // Assertion
        result.Should().Contain(r => r.Id == roomId);
    }

    [Fact]
    public async Task GetAvailableRoomsAsync_WithOverlap_ExcludesBookedRooms()
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

        var roomResult = await _service.CreateRoomAsync(organizer.Id, "Test Room", "123 Test St", 20);
        var roomId = roomResult.Room!.Id;

        var startTime = DateTime.UtcNow.AddDays(1).AddHours(10);
        var endTime = DateTime.UtcNow.AddDays(1).AddHours(12);
        var rentalResult = await _service.RequestRentalAsync(roomId, renter.Id, startTime, endTime);
        await _service.ApproveRentalAsync(rentalResult.Rental!.Id, organizer.Id);

        // Call: Check availability for overlapping time
        var result = await _service.GetAvailableRoomsAsync(startTime.AddMinutes(30), endTime.AddMinutes(30));

        // Assertion
        result.Should().NotContain(r => r.Id == roomId);
    }

    [Fact]
    public async Task GetAvailableRoomsAsync_MinCapacityFilter_ReturnsOnlySufficientCapacity()
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

        await _service.CreateRoomAsync(organizer.Id, "Small Room", "123 Test St", 10);
        await _service.CreateRoomAsync(organizer.Id, "Large Room", "456 Test St", 50);

        var startTime = DateTime.UtcNow.AddDays(1).AddHours(10);
        var endTime = DateTime.UtcNow.AddDays(1).AddHours(12);

        // Call: Filter by minimum capacity
        var result = await _service.GetAvailableRoomsAsync(startTime, endTime, minCapacity: 30);

        // Assertion
        result.Should().OnlyContain(r => r.Capacity >= 30);
        result.Should().Contain(r => r.Name == "Large Room");
        result.Should().NotContain(r => r.Name == "Small Room");
    }

    #endregion
}

