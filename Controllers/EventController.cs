using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay_Web.Models;
using Microsoft.EntityFrameworkCore; // Enables database query operations.
using RACEDAY_API_PROJECT.Data;
using RACEDAY_API_PROJECT.Models;
using System.Security.Claims;

namespace RaceDay_Web.Controllers;

[Authorize]
public class EventsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly IWebHostEnvironment _environment;

    public EventsController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
        public EventController(
            RaceDayDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

    public async Task<IActionResult> Index()
    {
        var client = _httpClientFactory.CreateClient("RaceDayApi");
        // ============================================================
        // CREATE EVENT
        // ============================================================

        [Authorize(Roles = "Organiser")]
        [HttpPost]
        public async Task<ActionResult<Event>> CreateEvent(Event newEvent)
        {
            // Get the ID of the organiser who is currently logged in.
            var organiserIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);
                ClaimTypes.NameIdentifier);
        try
        {
            var events = await client.GetFromJsonAsync<List<EventViewModel>>(
                "api/Event");
            if (organiserIdClaim == null)
            {
                return Unauthorized();
            }
                return Unauthorized();
            return View(events ?? new List<EventViewModel>());
            // Convert the organiser ID from string to integer.
            int organiserID = int.Parse(organiserIdClaim);

            // Use the logged-in organiser's ID.
            // Do not trust an organiserID sent from Swagger.
            newEvent.organiserID = organiserID;

            // Add the event to the database.
            _context.Events.Add(newEvent);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = newEvent.eventID },
                newEvent
            );
                nameof(GetEvent),
        catch (HttpRequestException ex)

        // ============================================================
        // GET A SINGLE EVENT
        // ============================================================

        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(int id)

            ViewBag.ErrorMessage = $"API connection error: {ex.Message}";
            return View(new List<EventViewModel>());
        }
        catch (System.Text.Json.JsonException ex)
        {
            ViewBag.ErrorMessage = $"API response error: {ex.Message}";
            return View(new List<EventViewModel>());
        }
    }
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
            }
    public async Task<IActionResult> Details(int id)
    {
        if (id <= 0)
        {
            return BadRequest();
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
            // Get the ID of the logged-in organiser.
            var organiserIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (organiserIdClaim == null)
            {
                return Unauthorized();
            }
            if (organiserIdClaim == null)
            {
                return Unauthorized();
        try
        {
            var eventItem = await client.GetFromJsonAsync<EventViewModel>(
                $"api/Event/{id}");
            // Find the event.
            var existingEvent = await _context.Events.FindAsync(id);
        var client = _httpClientFactory.CreateClient("RaceDayApi");
            if (eventItem == null)
            if (existingEvent == null)
            // Find the event that the Organiser wants to update.
            var existingEvent = await _context.Events.FindAsync(id);

            // If the event doesn't exist, return 404.
            return View(eventItem);
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                "Unable to retrieve the event from the RaceDay API.");
        }
        catch (System.Text.Json.JsonException)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                "The RaceDay API returned invalid event data.");
        }
    }
            // Check that the organiser owns this event.
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
            existingEvent.eventType = updatedEvent.eventType;
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> Enrol(
        CreateEnrolmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(
                "Details",
                new { id = model.eventID });
        }
        // ============================================================
        // DELETE EVENT
        // ============================================================

        [Authorize(Roles = "Organiser")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            // Get the ID of the logged-in organiser.
            var organiserIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (organiserIdClaim == null)
            {
                return Unauthorized();
            }
        [HttpDelete("{id}")]
        var apiCookie = User.FindFirst("RaceDayApiCookie")?.Value;
            int organiserID = int.Parse(organiserIdClaim);

            // Find the event.
            var raceEvent = await _context.Events.FindAsync(id);
            var organiserIdClaim = User.FindFirstValue(
        if (string.IsNullOrWhiteSpace(apiCookie))
        {
            TempData["ErrorMessage"] =
                "Your session has expired. Please log in again.";

            return RedirectToAction("Details",
                new { id = model.eventID });
        }
            if (raceEvent == null)
            {
                return NotFound();
            }
            }
        var client = _httpClientFactory.CreateClient("RaceDayApi");

        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "api/Enrolment");
            // Check that the organiser owns the event.
            if (raceEvent.organiserID != organiserID)
            {
                return Forbid();
            }

            // Remove the event from the database.
            _context.Events.Remove(raceEvent);
            {
            request.Headers.TryAddWithoutValidation(
                "Cookie",
                $"RaceDayAuth={apiCookie}");
            await _context.SaveChangesAsync();

            // Check that the logged-in Organiser owns this event.
            if (raceEvent.organiserID != organiserID)
            {
                return Forbid();

            using var response = await client.SendAsync(request);
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
            request.Content = JsonContent.Create(new
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] =
                    "You have successfully enrolled in this event category!";
            }
            else
            {
                TempData["ErrorMessage"] =
                    await response.Content.ReadAsStringAsync();
            }
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] =
                "Unable to connect to the RaceDay API.";
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

            // Only the event owner can view its enrolments.
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
 return Forbid();
}
        return RedirectToAction(
            "Details",
            new { id = model.eventID });
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

        // ============================================================
        // GET MY EVENTS
        // Only returns events created by the logged-in organiser.
        // ============================================================

        [Authorize(Roles = "Organiser")]
        [HttpGet("mine")]
        public async Task<ActionResult<IEnumerable<Event>>> GetMyEvents()
        {
            // Get the ID of the logged-in organiser.
            var organiserIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(organiserIdClaim, out int organiserID))
            {
                return Unauthorized();
            }

            // Retrieve only events belonging to this organiser.
            var myEvents = await _context.Events
                .Include(e => e.Categories)
                .Where(e => e.organiserID == organiserID)
                .ToListAsync();

            return Ok(myEvents);
        }
return Ok(enrolments);
}
        // ============================================================
        // UPLOAD EVENT BANNER
        // ============================================================

        [Authorize(Roles = "Organiser")]
        [HttpPost("{id}/banner")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UploadEventBanner(
            int id,
            IFormFile file)
        {
            var organiserIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(organiserIdClaim, out int organiserID))
            {
                return Unauthorized();
            }

            var raceEvent = await _context.Events.FindAsync(id);

            if (raceEvent == null)
            {
                return NotFound("Event not found.");
            }

            // Check that the organiser owns this event.
            if (raceEvent.organiserID != organiserID)
            {
                return Forbid();
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("Please select a banner image.");
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest("Banner image must not exceed 5 MB.");
            }

            // Only allow supported image formats.
            var allowedExtensions = new[]
            {
                ".jpg", ".jpeg", ".png", ".webp"
            };

            var extension = Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(
                    "Only JPG, JPEG, PNG and WEBP images are allowed.");
            }

            // Create the uploads folder if it does not exist.
            var webRoot = _environment.WebRootPath
                ?? Path.Combine(
                    _environment.ContentRootPath,
                    "wwwroot");

            var uploadsFolder = Path.Combine(
                webRoot,
                "uploads",
                "events");

            Directory.CreateDirectory(uploadsFolder);

            // Generate a unique filename.
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save the uploaded image.
            using (var stream = new FileStream(
                filePath,
                FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Delete the previous banner if one exists.
            if (!string.IsNullOrEmpty(raceEvent.bannerImagePath))
            {
                var oldFileName = Path.GetFileName(
                    raceEvent.bannerImagePath);

                var oldFilePath = Path.Combine(
                    uploadsFolder,
                    oldFileName);

                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            // Save the new banner path.
            raceEvent.bannerImagePath =
                $"/uploads/events/{fileName}";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event banner uploaded successfully.",
                bannerImagePath = raceEvent.bannerImagePath
            });
        }
    }
}

        {
            var events = await _context.Events
                .Include(e => e.Categories)
                .ToListAsync();

            return Ok(events);
        }

    }
}


