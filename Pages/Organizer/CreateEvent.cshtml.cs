using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;
using System.ComponentModel.DataAnnotations;

namespace CampusEvents.Pages.Organizer
{
    public class CreateEventModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateEventModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public List<Organization> Organizations { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Title is required")]
            [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
            public string Title { get; set; } = string.Empty;

            [Required(ErrorMessage = "Description is required")]
            [StringLength(2000, MinimumLength = 20, ErrorMessage = "Description must be between 20 and 2000 characters")]
            public string Description { get; set; } = string.Empty;

            [Required(ErrorMessage = "Category is required")]
            public EventCategory Category { get; set; }

            [Required(ErrorMessage = "Event date is required")]
            [DataType(DataType.Date)]
            public DateTime EventDate { get; set; }

            [Required(ErrorMessage = "Event time is required")]
            [DataType(DataType.Time)]
            public TimeSpan EventTime { get; set; }

            [Required(ErrorMessage = "Location is required")]
            [StringLength(300, ErrorMessage = "Location cannot exceed 300 characters")]
            public string Location { get; set; } = string.Empty;

            public int? OrganizationId { get; set; }

            [Required(ErrorMessage = "Capacity is required")]
            [Range(1, 10000, ErrorMessage = "Capacity must be between 1 and 10,000")]
            public int Capacity { get; set; }

            [Required(ErrorMessage = "Ticket type is required")]
            public string TicketType { get; set; } = "Free";

            [Range(0, 1000, ErrorMessage = "Price must be between 0 and 1,000")]
            public decimal Price { get; set; } = 0;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            // Check if user is an organizer
            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null || user.Role != UserRole.Organizer)
            {
                ErrorMessage = "Only organizers can create events.";
                return RedirectToPage("/Index");
            }

            // Check if organizer is approved
            if (user.ApprovalStatus != ApprovalStatus.Approved)
            {
                ErrorMessage = "Your organizer account is pending approval. You cannot create events yet.";
                return Page();
            }

            // Load organizations for dropdown
            Organizations = await _context.Organizations
                .OrderBy(o => o.Name)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            // Reload organizations for dropdown in case of validation error
            Organizations = await _context.Organizations
                .OrderBy(o => o.Name)
                .ToListAsync();

            // Validate user is an approved organizer
            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null || user.Role != UserRole.Organizer)
            {
                ErrorMessage = "Only organizers can create events.";
                return Page();
            }

            if (user.ApprovalStatus != ApprovalStatus.Approved)
            {
                ErrorMessage = "Your organizer account must be approved before creating events.";
                return Page();
            }

            // Custom validation
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Combine date and time
            var eventDateTime = Input.EventDate.Date.Add(Input.EventTime);

            // Ensure event is in the future
            if (eventDateTime <= DateTime.UtcNow)
            {
                ErrorMessage = "Event must be scheduled for a future date and time.";
                return Page();
            }

            // Validate price for paid events
            if (Input.TicketType == "Paid" && Input.Price <= 0)
            {
                ErrorMessage = "Price must be greater than 0 for paid events.";
                return Page();
            }

            // Create event
            var newEvent = new Event
            {
                Title = Input.Title,
                Description = Input.Description,
                Category = Input.Category,
                EventDate = eventDateTime,
                Location = Input.Location,
                Capacity = Input.Capacity,
                TicketType = Input.TicketType == "Free" ? TicketType.Free : TicketType.Paid,
                Price = Input.TicketType == "Free" ? 0 : Input.Price,
                OrganizerId = userId.Value,
                OrganizationId = Input.OrganizationId,
                ApprovalStatus = ApprovalStatus.Pending, // Requires admin approval
                CreatedAt = DateTime.UtcNow,
                TicketsIssued = 0
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Event created successfully! It is pending admin approval.";

            return RedirectToPage("/Organizer/Events");
        }
    }
}
