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

    // Get all active categories
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Ok(categories.Select(MapCategoryToDto));
    }

    // Get single category by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return NotFound(new { message = "Category not found" });
        }

        return Ok(MapCategoryToDto(category));
    }

    // Create new category (admin only)
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        // Check if user is admin
        if (!IsAdmin())
        {
            return Forbid("Only administrators can create categories");
        }

        // Basic validation
        if (string.IsNullOrEmpty(request.Name))
        {
            return BadRequest(new { message = "Category name is required" });
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

    // Update category (admin only)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
    {
        // Check if user is admin
        if (!IsAdmin())
        {
            return Forbid("Only administrators can update categories");
        }

        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound(new { message = "Category not found" });
        }

        // Update fields if provided
        if (!string.IsNullOrEmpty(request.Name))
            category.Name = request.Name;
        
        if (request.Description != null)
            category.Description = request.Description;
        
        if (request.IconName != null)
            category.IconName = request.IconName;
        
        if (request.IsActive.HasValue)
            category.IsActive = request.IsActive.Value;

        await _context.SaveChangesAsync();

        return Ok(MapCategoryToDto(category));
    }

    // Delete category (admin only) - soft delete
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        // Check if user is admin
        if (!IsAdmin())
        {
            return Forbid("Only administrators can delete categories");
        }

        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound(new { message = "Category not found" });
        }

        // Soft delete - set IsActive to false
        category.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Helper method to check if current user is admin
    private bool IsAdmin()
    {
        var role = HttpContext.Session.GetString("UserRole");
        return role == "Admin";
    }

    // Simple mapping to return category data
    private static object MapCategoryToDto(Category category) => new
    {
        category.Id,
        category.Name,
        category.Description,
        category.IconName,
        category.IsActive
    };
}

// Simple request classes
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

