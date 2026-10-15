
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
    public class EnrolmentController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentController(RaceDayDbContext context)
        {
            _context = context;
        }

        // POST: api/Enrolment
        // A Participant can enrol themselves in a category.
        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<ActionResult<Enrolment>> CreateEnrolment(
            Enrolment request)
        {
            string? userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int participantId))
                return Unauthorized();

            var category = await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.categoryID == request.categoryID);

            if (category == null)
                return NotFound("Category not found.");

            var enrolment = new Enrolment
            {
                participantID = participantId,
                categoryID = request.categoryID,
                enrolmentDate = DateTime.UtcNow,
                status = "Pending"
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Enrolment created successfully.",
                enrolment.enrolmentID,
                enrolment.participantID,
                enrolment.categoryID,
                enrolment.enrolmentDate,
                enrolment.status
            });
        }

        // GET: api/Enrolment/mine
        // A Participant can view all their own enrolments.
        [HttpGet("mine")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            string? userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int participantId))
                return Unauthorized();

            var enrolments = await _context.Enrolments
                .Where(e => e.participantID == participantId)
                .Select(e => new
                {
                    e.enrolmentID,
                    e.participantID,
                    e.categoryID,
                    e.enrolmentDate,
                    e.status
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: api/Enrolment/1
        // A Participant can view a specific enrolment they own.
        [HttpGet("{id}")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetEnrolment(int id)
        {
            string? userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int participantId))
                return Unauthorized();

            var enrolment = await _context.Enrolments
                .Where(e => e.enrolmentID == id)
                .Select(e => new
                {
                    e.enrolmentID,
                    e.participantID,
                    e.categoryID,
                    e.enrolmentDate,
                    e.status
                })
                .FirstOrDefaultAsync();

            if (enrolment == null)
                return NotFound("Enrolment not found.");

            if (enrolment.participantID != participantId)
                return Forbid();

            return Ok(enrolment);
        }

        // PUT: api/Enrolment/1
        // A Participant can update their own enrolment.
        [HttpPut("{id}")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> UpdateEnrolment(
            int id, Enrolment request)
        {
            string? userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int participantId))
                return Unauthorized();

            var enrolment = await _context.Enrolments
                .FirstOrDefaultAsync(e => e.enrolmentID == id);

            if (enrolment == null)
                return NotFound("Enrolment not found.");

            if (enrolment.participantID != participantId)
                return Forbid();

            var category = await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.categoryID == request.categoryID);

            if (category == null)
                return NotFound("Category not found.");

            enrolment.categoryID = request.categoryID;

            // Only allow the expected enrolment statuses.
            var allowedStatuses = new[]
            {
                "Pending",
                "Confirmed",
                "Rejected"
            };

            if (string.IsNullOrWhiteSpace(request.status) ||
                !allowedStatuses.Contains(request.status))
            {
                return BadRequest(
                    "Status must be Pending, Confirmed, or Rejected.");
            }

            enrolment.status = request.status;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Enrolment updated successfully.",
                enrolment.enrolmentID,
                enrolment.participantID,
                enrolment.categoryID,
                enrolment.enrolmentDate,
                enrolment.status
            });
        }

        // DELETE: api/Enrolment/1
        // A Participant can cancel their own enrolment.
        [HttpDelete("{id}")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> DeleteEnrolment(int id)
        {
            string? userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int participantId))
                return Unauthorized();

            var enrolment = await _context.Enrolments
                .FirstOrDefaultAsync(e => e.enrolmentID == id);

            if (enrolment == null)
                return NotFound("Enrolment not found.");

            if (enrolment.participantID != participantId)
                return Forbid();

            _context.Enrolments.Remove(enrolment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

