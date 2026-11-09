using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Enables database query operations.
using RACEDAY_API_PROJECT.Data; // Imports RaceDayDbContext.
using RACEDAY_API_PROJECT.Models; // Imports the project models.
using System.Security.Claims; // Reads the logged-in user's ID.

namespace RACEDAY_API_PROJECT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        // Constructor
        public EventController(RaceDayDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // CREATE EVENT
        // ============================================================

        [Authorize(Roles = "Organiser")]
        [HttpPost]
        public async Task<ActionResult<Event>> CreateEvent(Event newEvent)
        {
            // Get the ID of the logged-in Organiser.
            var organiserIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (organiserIdClaim == null)
            {
                return Unauthorized();
            }

            // Convert the organiser ID from string to integer.
            int organiserID = int.Parse(organiserIdClaim);

            // Use the ID of the logged-in Organiser.
            newEvent.organiserID = organiserID;

            // Add the event to the database.
            _context.Events.Add(newEvent);

            // Save the event.
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = newEvent.eventID },
                newEvent
            );
        }

        // ============================================================
        // GET A SINGLE EVENT
        // ============================================================

        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            // Anyone can view an event.
            var raceEvent = await _context.Events
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.eventID == id);

            if (raceEvent == null)
            {
                return NotFound();
            }

            return raceEvent;
        }

        // ============================================================
        // UPDATE EVENT
        // ============================================================

        [Authorize(Roles = "Organiser")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(
            int id,
            Event updatedEvent)
        {
            // Get the ID of the logged-in Organiser.
            var organiserIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (organiserIdClaim == null)
            {
                return Unauthorized();
            }

            int organiserID = int.Parse(organiserIdClaim);

            // Find the event.
            var existingEvent = await _context.Events.FindAsync(id);

            if (existingEvent == null)
            {
                return NotFound();
            }

            // Check that the Organiser owns this event.
            if (existingEvent.organiserID != organiserID)
            {
                return Forbid();
            }

            // Update the event information.
            existingEvent.eventName = updatedEvent.eventName;
            existingEvent.eventDate = updatedEvent.eventDate;
            existingEvent.location = updatedEvent.location;
            existingEvent.eventType = updatedEvent.eventType;
            existingEvent.description = updatedEvent.description;

            await _context.SaveChangesAsync();

            return Ok(existingEvent);
        }

        // ============================================================
        // DELETE EVENT
        // ============================================================

        [Authorize(Roles = "Organiser")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            // Get the ID of the logged-in Organiser.
            var organiserIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (organiserIdClaim == null)
            {
                return Unauthorized();
            }

            int organiserID = int.Parse(organiserIdClaim);

            // Find the event.
            var raceEvent = await _context.Events.FindAsync(id);

            if (raceEvent == null)
            {
                return NotFound();
            }

            // Check that the Organiser owns this event.
            if (raceEvent.organiserID != organiserID)
            {
                return Forbid();
            }

            // Remove the event from the database.
            _context.Events.Remove(raceEvent);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event deleted successfully."
            });
        }

        // ============================================================
        // GET EVENT ENROLMENTS
        // ============================================================

        [Authorize(Roles = "Organiser")]
        [HttpGet("{id}/enrolments")]
        public async Task<IActionResult> GetEventEnrolments(int id)
        {
            var organiserIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (organiserIdClaim == null)
            {
                return Unauthorized();
            }

            int organiserID = int.Parse(organiserIdClaim);

            var raceEvent = await _context.Events.FindAsync(id);

            if (raceEvent == null)
            {
                return NotFound("Event not found.");
            }

            if (raceEvent.organiserID != organiserID)
            {
                return Forbid();
            }

            var enrolments = await _context.Enrolments
                .Where(e => e.category.eventID == id)
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

        // ============================================================
        // GET ALL EVENTS
        // Used by participants to browse all events.
        // ============================================================

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            var events = await _context.Events
                .Include(e => e.Categories)
                .ToListAsync();

            return Ok(events);
        }
    }
}
