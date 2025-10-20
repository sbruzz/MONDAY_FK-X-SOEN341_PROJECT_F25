using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;

namespace CampusEvents.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Ok(categories.Select(MapCategoryToDto));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        return Ok(MapCategoryToDto(category));
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        // TODO: Check if user is admin
        var userId = GetCurrentUserId();
        if (userId == null || !IsAdmin(userId.Value))
        {
            return Unauthorized();
        }

        var category = new Category
        {
            Name = request.Name,
            Description = request.Description,
            IconName = request.IconName,
            IsActive = true
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, MapCategoryToDto(category));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
    {
        // TODO: Check if user is admin
        var userId = GetCurrentUserId();
        if (userId == null || !IsAdmin(userId.Value))
        {
            return Unauthorized();
        }

        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        category.Name = request.Name ?? category.Name;
        category.Description = request.Description ?? category.Description;
        category.IconName = request.IconName ?? category.IconName;
        category.IsActive = request.IsActive ?? category.IsActive;

        await _context.SaveChangesAsync();

        return Ok(MapCategoryToDto(category));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        // TODO: Check if user is admin
        var userId = GetCurrentUserId();
        if (userId == null || !IsAdmin(userId.Value))
        {
            return Unauthorized();
        }

        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        // Soft delete - set IsActive to false
        category.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private int? GetCurrentUserId()
    {
        // TODO: Implement proper session/auth check
        return null;
    }

    private bool IsAdmin(int userId)
    {
        // TODO: Implement admin check
        return false;
    }

    private static object MapCategoryToDto(Category category) => new
    {
        category.Id,
        category.Name,
        category.Description,
        category.IconName,
        category.IsActive
    };
}

public class CreateCategoryRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? IconName { get; set; }
}

public class UpdateCategoryRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? IconName { get; set; }
    public bool? IsActive { get; set; }
}
