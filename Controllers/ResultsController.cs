
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

        // GET: api/results/mine
        // Participants can view their own results.
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
                    resultID = r.resultID,
                    enrolmentID = r.enrolmentID,
                    finishTime = r.finishTime,
                    position = r.position
                })
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/events/1/results
        // Organisers can view results for their own events.
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
                    resultID = r.resultID,
                    enrolmentID = r.enrolmentID,
                    finishTime = r.finishTime,
                    position = r.position,

                    participantName = _context.Participants
                        .Where(p => p.participantID ==
                            r.Enrolment.participantID)
                        .Select(p => p.firstName + " " + p.lastName)
                        .FirstOrDefault() ?? "Unknown participant",

                    categoryName = r.Enrolment.category.categoryName
                })
                .ToListAsync();

            return Ok(results);
        }

        // POST: api/results
        // Organisers can record results for their own events.
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateResult(Result request)
        {
            string? userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int organiserId))
                return Unauthorized();

            var enrolment = await _context.Enrolments
                .Include(e => e.category)
                .FirstOrDefaultAsync(e =>
                    e.enrolmentID == request.enrolmentID);

            if (enrolment == null)
                return NotFound("Enrolment not found.");

            var eventItem = await _context.Events.FindAsync(
                enrolment.category!.eventID);

            if (eventItem == null)
                return NotFound("Event not found.");

            if (eventItem.organiserID != organiserId)
                return Forbid();

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
        // Organisers can update results for their own events.
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateResult(
            int id, Result request)
        {
            string? userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int organiserId))
                return Unauthorized();

            if (id != request.resultID)
                return BadRequest("Result ID does not match.");

            var result = await _context.Results
                .FirstOrDefaultAsync(r => r.resultID == id);

            if (result == null)
                return NotFound("Result not found.");

            var enrolment = await _context.Enrolments
                .Include(e => e.category)
                .FirstOrDefaultAsync(e =>
                    e.enrolmentID == result.enrolmentID);

            if (enrolment == null)
                return NotFound("Enrolment not found.");

            var eventItem = await _context.Events.FindAsync(
                enrolment.category!.eventID);

            if (eventItem == null)
                return NotFound("Event not found.");

            if (eventItem.organiserID != organiserId)
                return Forbid();

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
        // Any authenticated user can view a specific result.
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetResult(int id)
        {
            var result = await _context.Results
                .Where(r => r.resultID == id)
                .Select(r => new
                {
                    resultID = r.resultID,
                    enrolmentID = r.enrolmentID,
                    finishTime = r.finishTime,
                    position = r.position,

                    participantName = _context.Participants
                        .Where(p => p.participantID ==
                            r.Enrolment.participantID)
                        .Select(p => p.firstName + " " + p.lastName)
                        .FirstOrDefault() ?? "Unknown participant",

                    categoryName = r.Enrolment.category.categoryName
                })
                .FirstOrDefaultAsync();

            if (result == null)
                return NotFound("Result not found.");

            return Ok(result);
        }

        // DELETE: api/results/1
        // Only the organiser who owns the event can delete a result.
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
