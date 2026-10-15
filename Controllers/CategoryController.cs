
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RACEDAY_API_PROJECT.Data;
using RACEDAY_API_PROJECT.Models;
using System.Security.Claims;

namespace RACEDAY_API_PROJECT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
public class CategoryController : ControllerBase
{
 private readonly RaceDayDbContext _context;

public CategoryController(RaceDayDbContext context)
{
_context = context;
}

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return await _context.Categories
                .ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<ActionResult<Category>> CreateCategory(Category category)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int organiserId))
            {
                return Unauthorized();
            }

            var eventItem = await _context.Events
                .FindAsync(category.eventID);

            if (eventItem == null)
            {
                return NotFound("Event not found.");
            }

            if (eventItem.organiserID != organiserId)
            {
                return Forbid();
            }

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategory),
                new { id = category.categoryID },
                category);
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateCategory(int id, Category request)
        {
            if (id != request.categoryID)
            {
                return BadRequest("Category ID does not match.");
            }

            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int organiserId))
            {
                return Unauthorized();
            }

            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            var eventItem = await _context.Events
                .FindAsync(category.eventID);

            if (eventItem == null)
            {
                return NotFound("Event not found.");
            }

            if (eventItem.organiserID != organiserId)
            {
                return Forbid();
            }
            category.categoryName = request.categoryName;
            category.distance = request.distance;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int organiserId))
            {
                return Unauthorized();
            }

            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            var eventItem = await _context.Events
                .FindAsync(category.eventID);

            if (eventItem == null)
            {
                return NotFound("Event not found.");
            }

            if (eventItem.organiserID != organiserId)
            {
                return Forbid();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
    }
