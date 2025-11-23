using Xunit;
using FluentAssertions;
using CampusEvents.Services;
using CampusEvents.Data;
using CampusEvents.Models;
using CampusEvents.Tests.TestHelpers;

namespace CampusEvents.Tests.UnitTests.Services;

/// <summary>
/// Unit tests for ProximityService using equivalence partitioning and boundary testing
/// </summary>
public class ProximityServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ProximityService _service;

    public ProximityServiceTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _service = new ProximityService(_context);
    }

    public void Dispose()
    {
        InMemoryDbContextFactory.Dispose(_context);
    }

    #region CalculateDistance Tests

    [Fact]
    public void CalculateDistance_SamePoint_ReturnsZero()
    {
        // Setup: Same coordinates
        double lat = 45.4972;
        double lon = -73.5789;

        // Call
        var distance = _service.CalculateDistance(lat, lon, lat, lon);

        // Assertion
        distance.Should().BeApproximately(0.0, 0.001);
    }

    [Fact]
    public void CalculateDistance_KnownDistance_ReturnsCorrectValue()
    {
        // Setup: Concordia University to McGill University (known distance ~1.2km)
        // Concordia: 45.4972, -73.5789
        // McGill: 45.5048, -73.5772
        double concordiaLat = 45.4972;
        double concordiaLon = -73.5789;
        double mcgillLat = 45.5048;
        double mcgillLon = -73.5772;

        // Call
        var distance = _service.CalculateDistance(concordiaLat, concordiaLon, mcgillLat, mcgillLon);

        // Assertion: Should be approximately 1.2 km
        distance.Should().BeApproximately(1.2, 0.3); // Allow 0.3km tolerance
    }

    [Theory]
    [InlineData(-90.0, 0.0, 90.0, 0.0)]  // Boundary: South pole to North pole
    [InlineData(0.0, -180.0, 0.0, 180.0)] // Boundary: Opposite sides of equator
    [InlineData(0.0, 0.0, 0.0, 0.0)]      // Boundary: Origin point
    public void CalculateDistance_BoundaryCoordinates_ReturnsValidDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // Call
        var distance = _service.CalculateDistance(lat1, lon1, lat2, lon2);

        // Assertion: Should be non-negative and reasonable
        distance.Should().BeGreaterThanOrEqualTo(0);
        distance.Should().BeLessThan(20000); // Less than half Earth's circumference
    }

    [Fact]
    public void CalculateDistance_ValidCoordinates_ReturnsPositiveDistance()
    {
        // Setup: Two different valid points
        double lat1 = 45.0;
        double lon1 = -73.0;
        double lat2 = 46.0;
        double lon2 = -74.0;

        // Call
        var distance = _service.CalculateDistance(lat1, lon1, lat2, lon2);

        // Assertion
        distance.Should().BeGreaterThan(0);
    }

    #endregion

    #region CheckDriverEligibilityAsync Tests

    [Fact]
    public async Task CheckDriverEligibilityAsync_ActiveDriverNoOffer_ReturnsEligible()
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
        result.Reason.Should().Contain("Eligible");
    }

    [Fact]
    public async Task CheckDriverEligibilityAsync_NotADriver_ReturnsIneligible()
    {
        // Setup: User without driver profile
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

        // Call
        var result = await _service.CheckDriverEligibilityAsync(user.Id, eventEntity.Id);

        // Assertion
        result.Eligible.Should().BeFalse();
        result.Reason.Should().Contain("not registered as a driver");
    }

    [Fact]
    public async Task CheckDriverEligibilityAsync_InactiveDriver_ReturnsIneligible()
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

        var driver = new Driver
        {
            UserId = user.Id,
            Capacity = 4,
            VehicleType = VehicleType.Sedan,
            DriverType = DriverType.Student,
            Status = DriverStatus.Suspended // Inactive
        };
        _context.Drivers.Add(driver);
        await _context.SaveChangesAsync();

        // Call
        var result = await _service.CheckDriverEligibilityAsync(user.Id, eventEntity.Id);

        // Assertion
        result.Eligible.Should().BeFalse();
        result.Reason.Should().Contain("status");
    }

    [Fact]
    public async Task CheckDriverEligibilityAsync_ExistingOffer_ReturnsIneligible()
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

        var offer = new CarpoolOffer
        {
            DriverId = driver.Id,
            EventId = eventEntity.Id,
            SeatsAvailable = 4,
            DepartureInfo = "Metro",
            DepartureTime = DateTime.UtcNow.AddDays(7).AddHours(-1),
            Status = CarpoolOfferStatus.Active
        };
        _context.CarpoolOffers.Add(offer);
        await _context.SaveChangesAsync();

        // Call
        var result = await _service.CheckDriverEligibilityAsync(user.Id, eventEntity.Id);

        // Assertion
        result.Eligible.Should().BeFalse();
        result.Reason.Should().Contain("already have an active offer");
    }

    #endregion

    #region CheckPassengerEligibilityAsync Tests

    [Fact]
    public async Task CheckPassengerEligibilityAsync_AvailableOffers_ReturnsEligible()
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

        var offer = new CarpoolOffer
        {
            DriverId = driver.Id,
            EventId = eventEntity.Id,
            SeatsAvailable = 3,
            DepartureInfo = "Metro",
            DepartureTime = DateTime.UtcNow.AddDays(7).AddHours(-1),
            Status = CarpoolOfferStatus.Active
        };
        _context.CarpoolOffers.Add(offer);
        await _context.SaveChangesAsync();

        // Call
        var result = await _service.CheckPassengerEligibilityAsync(passengerUser.Id, eventEntity.Id);

        // Assertion
        result.Eligible.Should().BeTrue();
        result.NearbyOffers.Should().NotBeNull();
        result.NearbyOffers!.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CheckPassengerEligibilityAsync_NoOffers_ReturnsIneligible()
    {
        // Setup: Event with no carpool offers
        var passengerUser = new User
        {
            Email = "passenger@test.com",
            PasswordHash = "hash",
            Name = "Passenger",
            Role = UserRole.Student,
            ApprovalStatus = ApprovalStatus.Approved
        };
        _context.Users.Add(passengerUser);

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

        // Call
        var result = await _service.CheckPassengerEligibilityAsync(passengerUser.Id, eventEntity.Id);

        // Assertion
        result.Eligible.Should().BeFalse();
        result.Reason.Should().Contain("No carpool offers available");
    }

    [Fact]
    public async Task CheckPassengerEligibilityAsync_AlreadyInCarpool_ReturnsIneligible()
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

        var offer = new CarpoolOffer
        {
            DriverId = driver.Id,
            EventId = eventEntity.Id,
            SeatsAvailable = 3,
            DepartureInfo = "Metro",
            DepartureTime = DateTime.UtcNow.AddDays(7).AddHours(-1),
            Status = CarpoolOfferStatus.Active
        };
        _context.CarpoolOffers.Add(offer);
        await _context.SaveChangesAsync();

        var passenger = new CarpoolPassenger
        {
            OfferId = offer.Id,
            PassengerId = passengerUser.Id,
            Status = PassengerStatus.Confirmed
        };
        _context.CarpoolPassengers.Add(passenger);
        await _context.SaveChangesAsync();

        // Call
        var result = await _service.CheckPassengerEligibilityAsync(passengerUser.Id, eventEntity.Id);

        // Assertion
        result.Eligible.Should().BeFalse();
        result.Reason.Should().Contain("already in a carpool");
    }

    #endregion

    #region FormatDistance Tests

    [Theory]
    [InlineData(0.0, "0m")]           // Boundary: exactly 0
    [InlineData(0.5, "500m")]        // Below 1km
    [InlineData(0.999, "999m")]      // Boundary: just below 1km
    [InlineData(1.0, "1.0km")]       // Boundary: exactly 1km
    [InlineData(1.001, "1.0km")]     // Boundary: just above 1km
    [InlineData(5.5, "5.5km")]       // Above 1km
    public void FormatDistance_BoundaryValues_ReturnsCorrectFormat(double distanceKm, string expected)
    {
        // Call
        var result = _service.FormatDistance(distanceKm);

        // Assertion
        result.Should().Be(expected);
    }

    [Fact]
    public void FormatDistance_LessThanOneKm_ReturnsMeters()
    {
        // Setup: Distance less than 1km
        double distance = 0.5;

        // Call
        var result = _service.FormatDistance(distance);

        // Assertion
        result.Should().EndWith("m");
        result.Should().NotContain("km");
    }

    [Fact]
    public void FormatDistance_GreaterThanOneKm_ReturnsKilometers()
    {
        // Setup: Distance greater than 1km
        double distance = 2.5;

        // Call
        var result = _service.FormatDistance(distance);

        // Assertion
        result.Should().EndWith("km");
    }

    #endregion

    #region EstimateTravelTime Tests

    [Fact]
    public void EstimateTravelTime_ValidDistance_ReturnsTimeSpan()
    {
        // Setup: 40km at 40km/h = 1 hour
        double distance = 40.0;
        double speed = 40.0;

        // Call
        var time = _service.EstimateTravelTime(distance, speed);

        // Assertion
        time.Should().Be(TimeSpan.FromHours(1));
    }

    [Fact]
    public void EstimateTravelTime_ZeroDistance_ReturnsZero()
    {
        // Setup
        double distance = 0.0;

        // Call
        var time = _service.EstimateTravelTime(distance);

        // Assertion
        time.Should().Be(TimeSpan.Zero);
    }

    #endregion
}

