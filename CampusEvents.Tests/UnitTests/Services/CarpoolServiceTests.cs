using Xunit;
using FluentAssertions;
using CampusEvents.Services;
using CampusEvents.Data;
using CampusEvents.Models;
using CampusEvents.Tests.TestHelpers;

namespace CampusEvents.Tests.UnitTests.Services;

/// <summary>
/// Unit tests for CarpoolService using equivalence partitioning and boundary testing
/// </summary>
public class CarpoolServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly CarpoolService _service;

    public CarpoolServiceTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _service = new CarpoolService(_context);
    }

    public void Dispose()
    {
        InMemoryDbContextFactory.Dispose(_context);
    }

    #region RegisterDriverAsync Tests

    [Fact]
    public async Task RegisterDriverAsync_ValidInputs_ReturnsSuccess()
    {
        // Setup: Arrange
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

        // Call: Act
        var result = await _service.RegisterDriverAsync(
            userId: user.Id,
            capacity: 4,
            vehicleType: VehicleType.Sedan,
            driverType: DriverType.Student
        );

        // Assertion: Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("successful");
        result.Driver.Should().NotBeNull();
        result.Driver!.UserId.Should().Be(user.Id);
        result.Driver.Capacity.Should().Be(4);
        result.Driver.Status.Should().Be(DriverStatus.Pending);
    }

    [Fact]
    public async Task RegisterDriverAsync_InvalidUserId_ReturnsFailure()
    {
        // Setup: Non-existent user
        var nonExistentUserId = 999;

        // Call
        var result = await _service.RegisterDriverAsync(
            userId: nonExistentUserId,
            capacity: 4,
            vehicleType: VehicleType.Sedan,
            driverType: DriverType.Student
        );

        // Assertion
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
        result.Driver.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]  // Boundary: below valid range
    [InlineData(51)] // Boundary: above valid range
    [InlineData(-1)] // Invalid: negative
    public async Task RegisterDriverAsync_InvalidCapacityBoundary_ReturnsFailure(int capacity)
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
        var result = await _service.RegisterDriverAsync(
            userId: user.Id,
            capacity: capacity,
            vehicleType: VehicleType.Sedan,
            driverType: DriverType.Student
        );

        // Assertion
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("between 1 and 50");
    }

    [Theory]
    [InlineData(1)]  // Boundary: minimum valid
    [InlineData(50)] // Boundary: maximum valid
    [InlineData(25)] // Middle of valid range
    public async Task RegisterDriverAsync_ValidCapacityBoundary_ReturnsSuccess(int capacity)
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
        var result = await _service.RegisterDriverAsync(
            userId: user.Id,
            capacity: capacity,
            vehicleType: VehicleType.Sedan,
            driverType: DriverType.Student
        );

        // Assertion
        result.Success.Should().BeTrue();
        result.Driver!.Capacity.Should().Be(capacity);
    }

    [Fact]
    public async Task RegisterDriverAsync_DuplicateRegistration_ReturnsFailure()
    {
        // Setup: User already registered as driver
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

        await _service.RegisterDriverAsync(user.Id, 4, VehicleType.Sedan, DriverType.Student);

        // Call: Try to register again
        var result = await _service.RegisterDriverAsync(
            userId: user.Id,
            capacity: 5,
            vehicleType: VehicleType.SUV,
            driverType: DriverType.Student
        );

        // Assertion
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already registered");
    }

    [Fact]
    public async Task RegisterDriverAsync_StudentDriverWithNonStudentUser_ReturnsFailure()
    {
        // Setup: Organizer trying to register as Student driver
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
        var result = await _service.RegisterDriverAsync(
            userId: organizer.Id,
            capacity: 4,
            vehicleType: VehicleType.Sedan,
            driverType: DriverType.Student
        );

        // Assertion
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Only students can register as student drivers");
    }

    [Fact]
    public async Task RegisterDriverAsync_OrganizerDriverWithNonOrganizerUser_ReturnsFailure()
    {
        // Setup: Student trying to register as Organizer driver
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
        var result = await _service.RegisterDriverAsync(
            userId: student.Id,
            capacity: 4,
            vehicleType: VehicleType.Sedan,
            driverType: DriverType.Organizer
        );

        // Assertion
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Only organizers can register as organizer drivers");
    }

    #endregion

    #region UpdateDriverAsync Tests

    [Fact]
    public async Task UpdateDriverAsync_ValidUpdate_ReturnsSuccess()
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

        // Call
        var result = await _service.UpdateDriverAsync(
            driverId: driverId,
            capacity: 6,
            vehicleType: VehicleType.SUV,
            licensePlate: "ABC123"
        );

        // Assertion
        result.Success.Should().BeTrue();
        var updatedDriver = await _context.Drivers.FindAsync(driverId);
        updatedDriver!.Capacity.Should().Be(6);
        updatedDriver.VehicleType.Should().Be(VehicleType.SUV);
        updatedDriver.LicensePlate.Should().Be("ABC123");
    }

    [Fact]
    public async Task UpdateDriverAsync_InvalidDriverId_ReturnsFailure()
    {
        // Call
        var result = await _service.UpdateDriverAsync(
            driverId: 999,
            capacity: 5
        );

        // Assertion
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public async Task UpdateDriverAsync_InvalidCapacityBoundary_ReturnsFailure(int capacity)
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

        // Call
        var result = await _service.UpdateDriverAsync(driverId, capacity: capacity);

        // Assertion
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("between 1 and 50");
    }

    #endregion

    #region CreateOfferAsync Tests

    [Fact]
    public async Task CreateOfferAsync_ValidOffer_ReturnsSuccess()
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

        var eventEntity = new Event
        {
            Title = "Test Event",
            Description = "Test",
            EventDate = DateTime.UtcNow.AddDays(7),
            Location = "Test Location",
            Capacity = 100,
            TicketType = TicketType.Free,
            Category = "Test",
            OrganizerId = 1,
            ApprovalStatus = ApprovalStatus.Approved
        };
        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        var registerResult = await _service.RegisterDriverAsync(user.Id, 4, VehicleType.Sedan, DriverType.Student);
        var driver = await _context.Drivers.FindAsync(registerResult.Driver!.Id);
        driver!.Status = DriverStatus.Active;
        await _context.SaveChangesAsync();

        // Call
        var result = await _service.CreateOfferAsync(
            driverId: driver.Id,
            eventId: eventEntity.Id,
            departureInfo: "Metro Station",
            departureTime: DateTime.UtcNow.AddDays(7).AddHours(-1)
        );

        // Assertion
        result.Success.Should().BeTrue();
        result.Offer.Should().NotBeNull();
        result.Offer!.EventId.Should().Be(eventEntity.Id);
        result.Offer.DriverId.Should().Be(driver.Id);
    }

    [Fact]
    public async Task CreateOfferAsync_InactiveDriver_ReturnsFailure()
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

        var eventEntity = new Event
        {
            Title = "Test Event",
            Description = "Test",
            EventDate = DateTime.UtcNow.AddDays(7),
            Location = "Test Location",
            Capacity = 100,
            TicketType = TicketType.Free,
            Category = "Test",
            OrganizerId = 1,
            ApprovalStatus = ApprovalStatus.Approved
        };
        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        var registerResult = await _service.RegisterDriverAsync(user.Id, 4, VehicleType.Sedan, DriverType.Student);
        var driverId = registerResult.Driver!.Id;
        // Driver status remains Pending (not Active)

        // Call
        var result = await _service.CreateOfferAsync(
            driverId: driverId,
            eventId: eventEntity.Id,
            departureInfo: "Metro Station",
            departureTime: DateTime.UtcNow.AddDays(7).AddHours(-1)
        );

        // Assertion
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not active");
    }

    [Fact]
    public async Task CreateOfferAsync_NonExistentEvent_ReturnsFailure()
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
        var driver = await _context.Drivers.FindAsync(registerResult.Driver!.Id);
        driver!.Status = DriverStatus.Active;
        await _context.SaveChangesAsync();

        // Call
        var result = await _service.CreateOfferAsync(
            driverId: driver.Id,
            eventId: 999, // Non-existent event
            departureInfo: "Metro Station",
            departureTime: DateTime.UtcNow.AddDays(7).AddHours(-1)
        );

        // Assertion
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Event not found");
    }

    [Fact]
    public async Task CreateOfferAsync_DuplicateOffer_ReturnsFailure()
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

        var eventEntity = new Event
        {
            Title = "Test Event",
            Description = "Test",
            EventDate = DateTime.UtcNow.AddDays(7),
            Location = "Test Location",
            Capacity = 100,
            TicketType = TicketType.Free,
            Category = "Test",
            OrganizerId = 1,
            ApprovalStatus = ApprovalStatus.Approved
        };
        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        var registerResult = await _service.RegisterDriverAsync(user.Id, 4, VehicleType.Sedan, DriverType.Student);
        var driver = await _context.Drivers.FindAsync(registerResult.Driver!.Id);
        driver!.Status = DriverStatus.Active;
        await _context.SaveChangesAsync();

        await _service.CreateOfferAsync(driver.Id, eventEntity.Id, "Metro Station", DateTime.UtcNow.AddDays(7).AddHours(-1));

        // Call: Try to create duplicate offer
        var result = await _service.CreateOfferAsync(
            driverId: driver.Id,
            eventId: eventEntity.Id,
            departureInfo: "Different Location",
            departureTime: DateTime.UtcNow.AddDays(7).AddHours(-1)
        );

        // Assertion
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already have an active offer");
    }

    #endregion

    #region JoinOfferAsync Tests

    [Fact]
    public async Task JoinOfferAsync_ValidJoin_ReturnsSuccess()
    {
        // Setup
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
            OrganizerId = 1,
            ApprovalStatus = ApprovalStatus.Approved
        };
        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        var registerResult = await _service.RegisterDriverAsync(driverUser.Id, 4, VehicleType.Sedan, DriverType.Student);
        var driver = await _context.Drivers.FindAsync(registerResult.Driver!.Id);
        driver!.Status = DriverStatus.Active;
        await _context.SaveChangesAsync();

        var offerResult = await _service.CreateOfferAsync(driver.Id, eventEntity.Id, "Metro", DateTime.UtcNow.AddDays(7).AddHours(-1));
        var offerId = offerResult.Offer!.Id;

        // Call
        var result = await _service.JoinOfferAsync(offerId, passengerUser.Id);

        // Assertion
        result.Success.Should().BeTrue();
        var offer = await _context.CarpoolOffers.FindAsync(offerId);
        offer!.SeatsAvailable.Should().Be(3); // Was 4, now 3
    }

    [Fact]
    public async Task JoinOfferAsync_FullOffer_ReturnsFailure()
    {
        // Setup: Create offer with 1 seat, then fill it
        var driverUser = new User
        {
            Email = "driver@test.com",
            PasswordHash = "hash",
            Name = "Driver",
            Role = UserRole.Student,
            ApprovalStatus = ApprovalStatus.Approved
        };
        var passenger1 = new User
        {
            Email = "passenger1@test.com",
            PasswordHash = "hash",
            Name = "Passenger 1",
            Role = UserRole.Student,
            ApprovalStatus = ApprovalStatus.Approved
        };
        var passenger2 = new User
        {
            Email = "passenger2@test.com",
            PasswordHash = "hash",
            Name = "Passenger 2",
            Role = UserRole.Student,
            ApprovalStatus = ApprovalStatus.Approved
        };
        _context.Users.AddRange(driverUser, passenger1, passenger2);

        var eventEntity = new Event
        {
            Title = "Test Event",
            Description = "Test",
            EventDate = DateTime.UtcNow.AddDays(7),
            Location = "Test Location",
            Capacity = 100,
            TicketType = TicketType.Free,
            Category = "Test",
            OrganizerId = 1,
            ApprovalStatus = ApprovalStatus.Approved
        };
        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        var registerResult = await _service.RegisterDriverAsync(driverUser.Id, 1, VehicleType.Sedan, DriverType.Student);
        var driver = await _context.Drivers.FindAsync(registerResult.Driver!.Id);
        driver!.Status = DriverStatus.Active;
        await _context.SaveChangesAsync();

        var offerResult = await _service.CreateOfferAsync(driver.Id, eventEntity.Id, "Metro", DateTime.UtcNow.AddDays(7).AddHours(-1));
        var offerId = offerResult.Offer!.Id;

        await _service.JoinOfferAsync(offerId, passenger1.Id); // Fill the offer

        // Call: Try to join full offer
        var result = await _service.JoinOfferAsync(offerId, passenger2.Id);

        // Assertion
        result.Success.Should().BeFalse();
        result.Message.Should().ContainAny("No seats available", "no longer active", "is no longer active");
    }

    [Fact]
    public async Task JoinOfferAsync_DriverJoinsOwnOffer_ReturnsFailure()
    {
        // Setup
        var driverUser = new User
        {
            Email = "driver@test.com",
            PasswordHash = "hash",
            Name = "Driver",
            Role = UserRole.Student,
            ApprovalStatus = ApprovalStatus.Approved
        };
        _context.Users.Add(driverUser);

        var eventEntity = new Event
        {
            Title = "Test Event",
            Description = "Test",
            EventDate = DateTime.UtcNow.AddDays(7),
            Location = "Test Location",
            Capacity = 100,
            TicketType = TicketType.Free,
            Category = "Test",
            OrganizerId = 1,
            ApprovalStatus = ApprovalStatus.Approved
        };
        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        var registerResult = await _service.RegisterDriverAsync(driverUser.Id, 4, VehicleType.Sedan, DriverType.Student);
        var driver = await _context.Drivers.FindAsync(registerResult.Driver!.Id);
        driver!.Status = DriverStatus.Active;
        await _context.SaveChangesAsync();

        var offerResult = await _service.CreateOfferAsync(driver.Id, eventEntity.Id, "Metro", DateTime.UtcNow.AddDays(7).AddHours(-1));
        var offerId = offerResult.Offer!.Id;

        // Call: Driver tries to join own offer
        var result = await _service.JoinOfferAsync(offerId, driverUser.Id);

        // Assertion
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("cannot join your own");
    }

    #endregion

    #region CancelOfferAsync Tests

    [Fact]
    public async Task CancelOfferAsync_ValidCancel_ReturnsSuccess()
    {
        // Setup
        var driverUser = new User
        {
            Email = "driver@test.com",
            PasswordHash = "hash",
            Name = "Driver",
            Role = UserRole.Student,
            ApprovalStatus = ApprovalStatus.Approved
        };
        _context.Users.Add(driverUser);

        var eventEntity = new Event
        {
            Title = "Test Event",
            Description = "Test",
            EventDate = DateTime.UtcNow.AddDays(7),
            Location = "Test Location",
            Capacity = 100,
            TicketType = TicketType.Free,
            Category = "Test",
            OrganizerId = 1,
            ApprovalStatus = ApprovalStatus.Approved
        };
        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        var registerResult = await _service.RegisterDriverAsync(driverUser.Id, 4, VehicleType.Sedan, DriverType.Student);
        var driver = await _context.Drivers.FindAsync(registerResult.Driver!.Id);
        driver!.Status = DriverStatus.Active;
        await _context.SaveChangesAsync();

        var offerResult = await _service.CreateOfferAsync(driver.Id, eventEntity.Id, "Metro", DateTime.UtcNow.AddDays(7).AddHours(-1));
        var offerId = offerResult.Offer!.Id;

        // Call
        var result = await _service.CancelOfferAsync(offerId, driverUser.Id);

        // Assertion
        result.Success.Should().BeTrue();
        var offer = await _context.CarpoolOffers.FindAsync(offerId);
        offer!.Status.Should().Be(CarpoolOfferStatus.Cancelled);
    }

    [Fact]
    public async Task CancelOfferAsync_WithConfirmedPassengers_ReturnsFailure()
    {
        // Setup
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
            OrganizerId = 1,
            ApprovalStatus = ApprovalStatus.Approved
        };
        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        var registerResult = await _service.RegisterDriverAsync(driverUser.Id, 4, VehicleType.Sedan, DriverType.Student);
        var driver = await _context.Drivers.FindAsync(registerResult.Driver!.Id);
        driver!.Status = DriverStatus.Active;
        await _context.SaveChangesAsync();

        var offerResult = await _service.CreateOfferAsync(driver.Id, eventEntity.Id, "Metro", DateTime.UtcNow.AddDays(7).AddHours(-1));
        var offerId = offerResult.Offer!.Id;

        await _service.JoinOfferAsync(offerId, passengerUser.Id); // Add confirmed passenger

        // Call: Try to cancel with confirmed passengers
        var result = await _service.CancelOfferAsync(offerId, driverUser.Id);

        // Assertion
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("passengers have confirmed");
    }

    #endregion
}

