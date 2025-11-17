using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;
using CampusEvents.Services;

namespace CampusEvents.Pages.Admin;

public class EditUserModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly CarpoolService _carpoolService;
    private readonly RoomRentalService _roomRentalService;

    public EditUserModel(AppDbContext context, CarpoolService carpoolService, RoomRentalService roomRentalService)
    {
        _context = context;
        _carpoolService = carpoolService;
        _roomRentalService = roomRentalService;
    }

    [BindProperty]
    public new User User { get; set; } = null!;

    public List<Organization> Organizations { get; set; } = new();
    public Driver? DriverProfile { get; set; }
    public List<RoomRental> UserRentals { get; set; } = new();
    public List<Room> ManagedRooms { get; set; } = new();
    public List<CarpoolOffer> CarpoolOffersAsDriver { get; set; } = new();
    public List<CarpoolPassenger> CarpoolsAsPassenger { get; set; } = new();

    [TempData]
    public string? Message { get; set; }

    [TempData]
    public bool IsSuccess { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Login");
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        User = user;
        Organizations = await _context.Organizations.OrderBy(o => o.Name).ToListAsync();

        // Load driver profile if exists
        var allDrivers = await _carpoolService.GetAllDriversAsync();
        DriverProfile = allDrivers.FirstOrDefault(d => d.UserId == id);

        // Load carpool offers if user is a driver
        if (DriverProfile != null)
        {
            CarpoolOffersAsDriver = await _context.CarpoolOffers
                .Include(co => co.Event)
                .Include(co => co.Passengers)
                .Where(co => co.DriverId == DriverProfile.Id)
                .OrderByDescending(co => co.DepartureTime)
                .Take(5)
                .ToListAsync();
        }

        // Load carpool participation as passenger
        CarpoolsAsPassenger = await _context.CarpoolPassengers
            .Include(cp => cp.Offer)
                .ThenInclude(co => co.Event)
            .Include(cp => cp.Offer)
                .ThenInclude(co => co.Driver)
                    .ThenInclude(d => d.User)
            .Where(cp => cp.PassengerId == id)
            .OrderByDescending(cp => cp.Offer.DepartureTime)
            .Take(5)
            .ToListAsync();

        // Load room rentals (as renter)
        UserRentals = await _roomRentalService.GetUserRentalsAsync(id);

        // Load managed rooms (if organizer)
        if (User.Role == UserRole.Organizer)
        {
            ManagedRooms = await _context.Rooms
                .Include(r => r.Rentals)
                .Where(r => r.OrganizerId == id)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Organizations = await _context.Organizations.OrderBy(o => o.Name).ToListAsync();
            return Page();
        }

        var userToUpdate = await _context.Users.FindAsync(User.Id);
        if (userToUpdate == null)
        {
            Message = "User not found.";
            IsSuccess = false;
            return RedirectToPage("/Admin/Users");
        }

        // Update basic info
        userToUpdate.Name = User.Name;
        userToUpdate.Email = User.Email;
        userToUpdate.Role = User.Role;
        userToUpdate.ApprovalStatus = User.ApprovalStatus;

        // Update student fields
        if (User.Role == UserRole.Student)
        {
            userToUpdate.StudentId = User.StudentId;
            userToUpdate.Program = User.Program;
            userToUpdate.YearOfStudy = User.YearOfStudy;
            userToUpdate.PhoneNumber = User.PhoneNumber;
            userToUpdate.OrganizationId = null;
            userToUpdate.Position = null;
            userToUpdate.Department = null;
        }
        // Update organizer fields
        else if (User.Role == UserRole.Organizer)
        {
            userToUpdate.OrganizationId = User.OrganizationId;
            userToUpdate.Position = User.Position;
            userToUpdate.Department = User.Department;
            userToUpdate.PhoneNumber = User.PhoneNumber;
            userToUpdate.StudentId = null;
            userToUpdate.Program = null;
            userToUpdate.YearOfStudy = null;
        }
        // Clear both if admin
        else
        {
            userToUpdate.StudentId = null;
            userToUpdate.Program = null;
            userToUpdate.YearOfStudy = null;
            userToUpdate.PhoneNumber = null;
            userToUpdate.OrganizationId = null;
            userToUpdate.Position = null;
            userToUpdate.Department = null;
        }

        await _context.SaveChangesAsync();

        Message = $"User {userToUpdate.Name} has been updated successfully.";
        IsSuccess = true;
        return RedirectToPage("/Admin/Users");
    }

    public async Task<IActionResult> OnPostApproveDriverAsync(int driverId, int userId)
    {
        var result = await _carpoolService.ApproveDriverAsync(driverId);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToPage(new { id = userId });
    }

    public async Task<IActionResult> OnPostSuspendDriverAsync(int driverId, int userId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["ErrorMessage"] = "Reason is required to suspend a driver.";
            return RedirectToPage(new { id = userId });
        }

        var result = await _carpoolService.SuspendDriverAsync(driverId, reason);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToPage(new { id = userId });
    }

    public async Task<IActionResult> OnPostUnsuspendDriverAsync(int driverId, int userId)
    {
        var result = await _carpoolService.UnsuspendDriverAsync(driverId);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToPage(new { id = userId });
    }
}
