using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;//enables the database query operations used by the new method.
using RACEDAY_API_PROJECT.Data; // this imports the data folder so we can use the RaceDayDbContext.
using RACEDAY_API_PROJECT.Models; // this allows us to use the models we created.
using System.Security.Claims; // this allows us to read the logged-in user's ID from the authentication cookie.

namespace RACEDAY_API_PROJECT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly RaceDayDbContext _context; // helps us to delete, read and update data.

        public EventController(RaceDayDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Organiser")]
        // Only a logged-in Organiser can create an event.

        [HttpPost]
        // This allows us to add a new event to the database.

        public async Task<ActionResult<Event>> CreateEvent(Event newEvent)
        {
            // Get the ID of the Organiser who is currently logged in.
            // This ID comes from the RaceDayAuth cookie.
            var organiserIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            // If the ID cannot be found, the user is not authenticated.
            if (organiserIdClaim == null)
            {
                return Unauthorized();
            }

            // Convert the ID from the cookie from a string to an integer.
            int organiserID = int.Parse(organiserIdClaim);

            // We use the ID of the logged-in Organiser.
            // We do not trust an organiserID sent from Swagger.
            newEvent.organiserID = organiserID;

            // Adds the new event to the Events table.
            _context.Events.Add(newEvent);

            // Saves the new event to the database.
            await _context.SaveChangesAsync();

            // Returns 201 Created after the event is successfully added.
            return CreatedAtAction(
                nameof(GetEvent),
                new { id = newEvent.eventID },
                newEvent
            );
        }

        [HttpGet("{id}")]
        // Anyone can view an event.
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var raceEvent = await _context.Events
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.eventID == id);

            if (raceEvent == null)
            {
                return NotFound();
            }

            return raceEvent;
        }

        [Authorize(Roles = "Organiser")]
        // Only Organisers can update events.

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

            // Find the event that the Organiser wants to update.
            var existingEvent = await _context.Events.FindAsync(id);

            // If the event doesn't exist, return 404.
            if (existingEvent == null)
            {
                return NotFound();
            }

            // Check that the logged-in Organiser actually owns
            // this event.
            if (existingEvent.organiserID != organiserID)
            {
                // The user is logged in, but does not own this event.
                return Forbid();
            }

            // Update the event information.
            existingEvent.eventName = updatedEvent.eventName;
            existingEvent.eventDate = updatedEvent.eventDate;
            existingEvent.location = updatedEvent.location;
            existingEvent.eventType = updatedEvent.eventType;
            existingEvent.description = updatedEvent.description;

            // Save the changes to SQL Server.
            await _context.SaveChangesAsync();

            return Ok(existingEvent);
        }


        // ============================================================
        // DELETE EVENT
        // ============================================================

        [Authorize(Roles = "Organiser")]
        // Only Organisers can delete events.

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

            // Find the event in the database.
            var raceEvent = await _context.Events.FindAsync(id);

            // If the event doesn't exist, return 404.
            if (raceEvent == null)
            {
                return NotFound();
            }

            // Check that the logged-in Organiser owns this event.
            if (raceEvent.organiserID != organiserID)
            {
                return Forbid();
            }

            // Remove the event from the database.
            _context.Events.Remove(raceEvent);

            // Save the deletion.
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event deleted successfully."
            });
        }
    
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
var enrolments = await _context.Enrolments.Where(e => e.category.eventID == id).Select(e => new
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
    }
}


