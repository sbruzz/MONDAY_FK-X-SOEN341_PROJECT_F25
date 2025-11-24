using CampusEvents.Data;
using CampusEvents.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusEvents.Services;

/// <summary>
/// Service for managing room rental system (US.04)
/// Handles room creation, rental requests, and availability
/// </summary>
public class RoomRentalService
{
    private readonly AppDbContext _context;
    private readonly NotificationService? _notificationService;

    public RoomRentalService(AppDbContext context, NotificationService? notificationService = null)
    {
        _context = context;
        _notificationService = notificationService;
    }

    // ===== Room Management =====

    /// <summary>
    /// Create a new room (by organizer)
    /// </summary>
    public async Task<(bool Success, string Message, Room? Room)> CreateRoomAsync(
        int organizerId,
        string name,
        string address,
        int capacity,
        string? roomInfo = null,
        string amenities = "",
        decimal? hourlyRate = null,
        DateTime? availabilityStart = null,
        DateTime? availabilityEnd = null)
    {
        // Validate organizer exists and is an organizer
        var user = await _context.Users.FindAsync(organizerId);
        if (user == null)
            return (false, "User not found", null);

        if (user.Role != UserRole.Organizer)
            return (false, "Only organizers can create rooms", null);

        // Validate capacity
        if (capacity < 1)
            return (false, "Capacity must be at least 1", null);

        // Validate availability window
        if (availabilityStart.HasValue && availabilityEnd.HasValue)
        {
            if (availabilityEnd.Value <= availabilityStart.Value)
                return (false, "Availability end time must be after start time", null);
        }

        // Create room
        var room = new Room
        {
            OrganizerId = organizerId,
            Name = name,
            Address = address,
            Capacity = capacity,
            RoomInfo = roomInfo,
            Amenities = amenities,
            HourlyRate = hourlyRate,
            Status = RoomStatus.Enabled, // Default enabled, admin can disable
            AvailabilityStart = availabilityStart,
            AvailabilityEnd = availabilityEnd
        };

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        return (true, "Room created successfully", room);
    }

    /// <summary>
    /// Update room details
    /// </summary>
    public async Task<(bool Success, string Message)> UpdateRoomAsync(
        int roomId,
        int userId,
        string? name = null,
        string? address = null,
        int? capacity = null,
        string? roomInfo = null,
        string? amenities = null,
        decimal? hourlyRate = null,
        DateTime? availabilityStart = null,
        DateTime? availabilityEnd = null)
    {
        var room = await _context.Rooms
            .FirstOrDefaultAsync(r => r.Id == roomId);

        if (room == null)
            return (false, "Room not found");

        // Verify user is the organizer who created the room
        if (room.OrganizerId != userId)
            return (false, "Only the room organizer can update this room");

        // Apply updates
        if (name != null) room.Name = name;
        if (address != null) room.Address = address;
        if (capacity.HasValue)
        {
            if (capacity.Value < 1)
                return (false, "Capacity must be at least 1");
            room.Capacity = capacity.Value;
        }
        if (roomInfo != null) room.RoomInfo = roomInfo;
        if (amenities != null) room.Amenities = amenities;
        if (hourlyRate.HasValue) room.HourlyRate = hourlyRate;
        if (availabilityStart.HasValue) room.AvailabilityStart = availabilityStart;
        if (availabilityEnd.HasValue) room.AvailabilityEnd = availabilityEnd;

        await _context.SaveChangesAsync();
        return (true, "Room updated successfully");
    }

    /// <summary>
    /// Get all rooms with optional filters
    /// </summary>
    public async Task<List<Room>> GetRoomsAsync(
        bool? onlyEnabled = true,
        int? minCapacity = null,
        DateTime? availableFrom = null,
        DateTime? availableTo = null)
    {
        var query = _context.Rooms
            .Include(r => r.Organizer)
            .Include(r => r.Rentals)
            .AsQueryable();

        if (onlyEnabled == true)
            query = query.Where(r => r.Status == RoomStatus.Enabled);

        if (minCapacity.HasValue)
            query = query.Where(r => r.Capacity >= minCapacity.Value);

        // Filter by availability window
        if (availableFrom.HasValue && availableTo.HasValue)
        {
            query = query.Where(r =>
                (!r.AvailabilityStart.HasValue || r.AvailabilityStart.Value <= availableFrom.Value) &&
                (!r.AvailabilityEnd.HasValue || r.AvailabilityEnd.Value >= availableTo.Value));
        }

        return await query
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Get rooms managed by a specific organizer
    /// </summary>
    public async Task<List<Room>> GetOrganizerRoomsAsync(int organizerId)
    {
        return await _context.Rooms
            .Include(r => r.Rentals)
                .ThenInclude(rr => rr.Renter)
            .Where(r => r.OrganizerId == organizerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    // ===== Rental Management =====

    /// <summary>
    /// Request a room rental
    /// Checks for double booking (overlapping approved/pending rentals)
    /// </summary>
    public async Task<(bool Success, string Message, RoomRental? Rental)> RequestRentalAsync(
        int roomId,
        int renterId,
        DateTime startTime,
        DateTime endTime,
        string? purpose = null,
        int? expectedAttendees = null)
    {
        // Validate room exists and is enabled
        var room = await _context.Rooms
            .Include(r => r.Rentals)
            .FirstOrDefaultAsync(r => r.Id == roomId);

        if (room == null)
            return (false, "Room not found", null);

        if (room.Status != RoomStatus.Enabled)
            return (false, "Room is currently disabled", null);

        // Validate time range
        if (endTime <= startTime)
            return (false, "End time must be after start time", null);

        if (startTime < DateTime.UtcNow)
            return (false, "Cannot book time in the past", null);

        // Validate availability window
        if (room.AvailabilityStart.HasValue && startTime < room.AvailabilityStart.Value)
            return (false, $"Room is not available before {room.AvailabilityStart.Value:yyyy-MM-dd HH:mm}", null);

        if (room.AvailabilityEnd.HasValue && endTime > room.AvailabilityEnd.Value)
            return (false, $"Room is not available after {room.AvailabilityEnd.Value:yyyy-MM-dd HH:mm}", null);

        // Validate capacity
        if (expectedAttendees.HasValue && expectedAttendees.Value > room.Capacity)
            return (false, $"Expected attendees ({expectedAttendees}) exceeds room capacity ({room.Capacity})", null);

        // Check for double booking (overlapping approved or pending rentals)
        var hasOverlap = await _context.RoomRentals
            .AnyAsync(rr => rr.RoomId == roomId &&
                           (rr.Status == RentalStatus.Approved || rr.Status == RentalStatus.Pending) &&
                           ((startTime >= rr.StartTime && startTime < rr.EndTime) ||
                            (endTime > rr.StartTime && endTime <= rr.EndTime) ||
                            (startTime <= rr.StartTime && endTime >= rr.EndTime)));

        if (hasOverlap)
            return (false, "Room is already booked for this time slot", null);

        // Calculate total cost
        decimal? totalCost = null;
        if (room.HourlyRate.HasValue)
        {
            var hours = (decimal)(endTime - startTime).TotalHours;
            totalCost = hours * room.HourlyRate.Value;
        }

        // Create rental request
        var rental = new RoomRental
        {
            RoomId = roomId,
            RenterId = renterId,
            StartTime = startTime,
            EndTime = endTime,
            Status = RentalStatus.Pending, // Requires approval
            Purpose = purpose,
            ExpectedAttendees = expectedAttendees,
            TotalCost = totalCost
        };

        _context.RoomRentals.Add(rental);
        await _context.SaveChangesAsync();

        return (true, "Rental request submitted successfully. Pending approval.", rental);
    }

    /// <summary>
    /// Approve a rental request (by organizer or admin)
    /// </summary>
    public async Task<(bool Success, string Message)> ApproveRentalAsync(
        int rentalId,
        int approverId,
        bool isAdmin = false)
    {
        var rental = await _context.RoomRentals
            .Include(rr => rr.Room)
            .FirstOrDefaultAsync(rr => rr.Id == rentalId);

        if (rental == null)
            return (false, "Rental request not found");

        // Verify approver is the room organizer or admin
        if (!isAdmin && rental.Room.OrganizerId != approverId)
            return (false, "Only the room organizer or admin can approve this rental");

        // Check if room is still enabled (admin disable overrides)
        if (rental.Room.Status != RoomStatus.Enabled)
            return (false, "Room has been disabled by administrator");

        // Double-check for overlaps (in case another rental was approved while this was pending)
        var hasOverlap = await _context.RoomRentals
            .AnyAsync(rr => rr.RoomId == rental.RoomId &&
                           rr.Id != rentalId &&
                           rr.Status == RentalStatus.Approved &&
                           ((rental.StartTime >= rr.StartTime && rental.StartTime < rr.EndTime) ||
                            (rental.EndTime > rr.StartTime && rental.EndTime <= rr.EndTime) ||
                            (rental.StartTime <= rr.StartTime && rental.EndTime >= rr.EndTime)));

        if (hasOverlap)
            return (false, "Cannot approve: conflicting rental was already approved");

        rental.Status = RentalStatus.Approved;
        await _context.SaveChangesAsync();

        // Send notification to renter
        if (_notificationService != null)
        {
            await _notificationService.NotifyRentalApprovedAsync(rentalId);
        }

        return (true, "Rental approved successfully");
    }

    /// <summary>
    /// Reject a rental request
    /// </summary>
    public async Task<(bool Success, string Message)> RejectRentalAsync(
        int rentalId,
        int rejecterId,
        string? adminNotes = null,
        bool isAdmin = false)
    {
        var rental = await _context.RoomRentals
            .Include(rr => rr.Room)
            .FirstOrDefaultAsync(rr => rr.Id == rentalId);

        if (rental == null)
            return (false, "Rental request not found");

        // Verify rejecter is the room organizer or admin
        if (!isAdmin && rental.Room.OrganizerId != rejecterId)
            return (false, "Only the room organizer or admin can reject this rental");

        rental.Status = RentalStatus.Rejected;
        rental.AdminNotes = adminNotes;
        await _context.SaveChangesAsync();

        // Send notification to renter
        if (_notificationService != null)
        {
            await _notificationService.NotifyRentalRejectedAsync(rentalId, adminNotes);
        }

        return (true, "Rental rejected");
    }

    /// <summary>
    /// Cancel a rental (by renter)
    /// </summary>
    public async Task<(bool Success, string Message)> CancelRentalAsync(int rentalId, int userId)
    {
        var rental = await _context.RoomRentals.FindAsync(rentalId);

        if (rental == null)
            return (false, "Rental not found");

        // Verify user is the renter
        if (rental.RenterId != userId)
            return (false, "Only the renter can cancel this rental");

        // Can only cancel pending or approved rentals
        if (rental.Status != RentalStatus.Pending && rental.Status != RentalStatus.Approved)
            return (false, "Cannot cancel rental with current status");

        rental.Status = RentalStatus.Cancelled;
        await _context.SaveChangesAsync();

        return (true, "Rental cancelled successfully");
    }

    /// <summary>
    /// Admin cancel a rental (can cancel any rental)
    /// </summary>
    public async Task<(bool Success, string Message)> AdminCancelRentalAsync(int rentalId, string? reason = null)
    {
        var rental = await _context.RoomRentals.FindAsync(rentalId);

        if (rental == null)
            return (false, "Rental not found");

        // Can only cancel pending or approved rentals
        if (rental.Status != RentalStatus.Pending && rental.Status != RentalStatus.Approved)
            return (false, "Cannot cancel rental with current status");

        rental.Status = RentalStatus.Cancelled;
        if (!string.IsNullOrWhiteSpace(reason))
            rental.AdminNotes = $"Cancelled by admin: {reason}";

        await _context.SaveChangesAsync();

        return (true, "Rental cancelled successfully by administrator");
    }

    /// <summary>
    /// Get user's rental history
    /// </summary>
    public async Task<List<RoomRental>> GetUserRentalsAsync(int userId)
    {
        return await _context.RoomRentals
            .Include(rr => rr.Room)
                .ThenInclude(r => r.Organizer)
            .Where(rr => rr.RenterId == userId)
            .OrderByDescending(rr => rr.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Get pending rental requests for organizer's rooms
    /// </summary>
    public async Task<List<RoomRental>> GetPendingRentalsForOrganizerAsync(int organizerId)
    {
        return await _context.RoomRentals
            .Include(rr => rr.Room)
            .Include(rr => rr.Renter)
            .Where(rr => rr.Room.OrganizerId == organizerId && rr.Status == RentalStatus.Pending)
            .OrderBy(rr => rr.StartTime)
            .ToListAsync();
    }

    /// <summary>
    /// Check room availability for a time slot
    /// Returns available rooms
    /// </summary>
    public async Task<List<Room>> GetAvailableRoomsAsync(
        DateTime startTime,
        DateTime endTime,
        int? minCapacity = null)
    {
        // Get all enabled rooms
        var rooms = await GetRoomsAsync(
            onlyEnabled: true,
            minCapacity: minCapacity,
            availableFrom: startTime,
            availableTo: endTime);

        // Filter out rooms with overlapping approved rentals
        var availableRooms = new List<Room>();

        foreach (var room in rooms)
        {
            var hasOverlap = await _context.RoomRentals
                .AnyAsync(rr => rr.RoomId == room.Id &&
                               (rr.Status == RentalStatus.Approved || rr.Status == RentalStatus.Pending) &&
                               ((startTime >= rr.StartTime && startTime < rr.EndTime) ||
                                (endTime > rr.StartTime && endTime <= rr.EndTime) ||
                                (startTime <= rr.StartTime && endTime >= rr.EndTime)));

            if (!hasOverlap)
                availableRooms.Add(room);
        }

        return availableRooms;
    }

    // ===== Admin Functions =====

    /// <summary>
    /// Admin enables a room
    /// </summary>
    public async Task<(bool Success, string Message)> EnableRoomAsync(int roomId)
    {
        var room = await _context.Rooms.FindAsync(roomId);
        if (room == null)
            return (false, "Room not found");

        room.Status = RoomStatus.Enabled;
        await _context.SaveChangesAsync();

        return (true, "Room enabled successfully");
    }

    /// <summary>
    /// Admin disables a room
    /// Admin disable overrides all rental status
    /// </summary>
    public async Task<(bool Success, string Message)> DisableRoomAsync(int roomId, string reason)
    {
        var room = await _context.Rooms
            .Include(r => r.Rentals)
            .FirstOrDefaultAsync(r => r.Id == roomId);

        if (room == null)
            return (false, "Room not found");

        room.Status = RoomStatus.Disabled;

        // Cancel all pending rentals
        foreach (var rental in room.Rentals.Where(r => r.Status == RentalStatus.Pending))
        {
            rental.Status = RentalStatus.Rejected;
            rental.AdminNotes = $"Room disabled by admin: {reason}";
        }

        await _context.SaveChangesAsync();

        // Send notifications to students with approved rentals for this room
        if (_notificationService != null)
        {
            await _notificationService.NotifyRoomDisabledAsync(roomId);
        }

        return (true, "Room disabled successfully. All pending rentals have been rejected.");
    }

    /// <summary>
    /// Get all rooms (for admin)
    /// </summary>
    public async Task<List<Room>> GetAllRoomsAsync(RoomStatus? status = null)
    {
        var query = _context.Rooms
            .Include(r => r.Organizer)
            .Include(r => r.Rentals)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Get all rental requests (for admin)
    /// </summary>
    public async Task<List<RoomRental>> GetAllRentalsAsync(RentalStatus? status = null)
    {
        var query = _context.RoomRentals
            .Include(rr => rr.Room)
            .Include(rr => rr.Renter)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(rr => rr.Status == status.Value);

        return await query
            .OrderByDescending(rr => rr.CreatedAt)
            .ToListAsync();
    }
}
