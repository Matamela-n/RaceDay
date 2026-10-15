using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RACEDAY_API_PROJECT.Data;
using RACEDAY_API_PROJECT.Models;
using System.Security.Claims;

namespace RACEDAY_API_PROJECT.Controllers
{
    [Route("api/results")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }
        [HttpGet("mine")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            string? userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int participantId))
                return Unauthorized();

            var results = await _context.Results
                .Where(r => r.Enrolment.participantID == participantId)
                .Select(r => new
                {
                    r.resultID,
                    r.enrolmentID,
                    r.finishTime,
                    r.position
                })
                .ToListAsync();

            return Ok(results);
        }
        [HttpGet("/api/events/{id}/results")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetEventResults(int id)
        {
            string? userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int organiserId))
                return Unauthorized();

            var eventItem = await _context.Events.FindAsync(id);

            if (eventItem == null)
                return NotFound("Event not found.");

            if (eventItem.organiserID != organiserId)
                return Forbid();

            var results = await _context.Results
                .Where(r => r.Enrolment.category.eventID == id)
                .Select(r => new
                {
                    r.resultID,
                    r.enrolmentID,
                    r.finishTime,
                    r.position
                })
                .ToListAsync();

            return Ok(results);
        }

        // POST: api/results
        // Only an Organiser can record a result for their own event.
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateResult(Result request)
        {
            // Get the logged-in Organiser's ID.
            string? userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int organiserId))
                return Unauthorized();

            // Find the enrolment that this result belongs to.
            var enrolment = await _context.Enrolments
                .Include(e => e.category)
                .FirstOrDefaultAsync(e =>
                    e.enrolmentID == request.enrolmentID);

            if (enrolment == null)
                return NotFound("Enrolment not found.");

            // Find the event through the enrolment's category.
            var eventItem = await _context.Events.FindAsync(
                enrolment.category!.eventID);

            if (eventItem == null)
                return NotFound("Event not found.");

            // Make sure the Organiser owns the event.
            if (eventItem.organiserID != organiserId)
                return Forbid();

            // Create the result using the submitted result details.
            var result = new Result
            {
                enrolmentID = request.enrolmentID,
                finishTime = request.finishTime,
                position = request.position
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Result recorded successfully.",
                result.resultID,
                result.enrolmentID,
                result.finishTime,
                result.position
            });
        }


        // PUT: api/results/1
        // Only the Organiser who owns the event can update a result.
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateResult(
            int id, Result request)
        {
            // Get the logged-in Organiser's ID.
            string? userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int organiserId))
                return Unauthorized();

            // Make sure the URL ID matches the result ID.
            if (id != request.resultID)
                return BadRequest("Result ID does not match.");

            // Find the existing result.
            var result = await _context.Results
                .FirstOrDefaultAsync(r => r.resultID == id);

            if (result == null)
                return NotFound("Result not found.");

            // Find the enrolment linked to the result.
            var enrolment = await _context.Enrolments
                .Include(e => e.category)
                .FirstOrDefaultAsync(e =>
                    e.enrolmentID == result.enrolmentID);

            if (enrolment == null)
                return NotFound("Enrolment not found.");

            // Find the event linked to the category.
            var eventItem = await _context.Events.FindAsync(
                enrolment.category!.eventID);

            if (eventItem == null)
                return NotFound("Event not found.");

            // Confirm that the Organiser owns this event.
            if (eventItem.organiserID != organiserId)
                return Forbid();

            // Update the finish time and finishing position.
            result.finishTime = request.finishTime;
            result.position = request.position;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Result updated successfully.",
                result.resultID,
                result.enrolmentID,
                result.finishTime,
                result.position
            });
        }



        // GET: api/results/1
        // Any logged-in user can view a specific result.
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetResult(int id)
        {
            var result = await _context.Results
                .Where(r => r.resultID == id)
                .Select(r => new
                {
                    r.resultID,
                    r.enrolmentID,
                    r.finishTime,
                    r.position
                })
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return NotFound("Result not found.");
            }

            return Ok(result);
        }


    

        // DELETE: api/results/1
        // Only the Organiser who owns the event can delete a result.
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteResult(int id)
        {
            string? userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int organiserId))
                return Unauthorized();

            var result = await _context.Results
                .FirstOrDefaultAsync(r => r.resultID == id);

            if (result == null)
                return NotFound("Result not found.");

            var enrolment = await _context.Enrolments
                .Include(e => e.category)
                .FirstOrDefaultAsync(e =>
                    e.enrolmentID == result.enrolmentID);

            if (enrolment?.category == null)
                return NotFound("Enrolment or category not found.");

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e =>
                    e.eventID == enrolment.category.eventID);

            if (eventItem == null)
                return NotFound("Event not found.");

            if (eventItem.organiserID != organiserId)
                return Forbid();

            _context.Results.Remove(result);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Result deleted successfully."
            });
        }

    }
}