
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RACEDAY_API_PROJECT.Data;
using RACEDAY_API_PROJECT.Models;
using System.Security.Claims;

namespace RACEDAY_API_PROJECT.Controllers
{
    // Both Organisers and Participants must be logged in.
    [Route("api/users/me")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ProfileController(RaceDayDbContext context)
        {
            // Gives this controller access to the database.
            _context = context;
        }

        // GET /api/users/me
        // Returns the logged-in user's own profile.
        [HttpGet]
        public async Task<ActionResult<UserProfileDto>> GetProfile()
        {
            string? userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            string? role =
                User.FindFirstValue(ClaimTypes.Role);

            if (!int.TryParse(userId, out int id))
            {
                return Unauthorized();
            }

            // Find the Organiser in the EventOrganisers table.
            if (role == "Organiser")
            {
                var organiser = await _context.EventOrganisers
                    .FindAsync(id);

                if (organiser == null)
                {
                    return NotFound();
                }

 return Ok(new UserProfileDto
{
  firstName = organiser.firstName,
  lastName = organiser.lastName,
email = organiser.email
                });
            }

            // Find the Participant in the Participants table.
if (role == "Participant")
{
var participant = await _context.Participants.FindAsync(id);

if (participant == null)
{
return NotFound();
}

                return Ok(new UserProfileDto
                {
                    firstName = participant.firstName,
                    lastName = participant.lastName,
                    email = participant.email
                });
            }

            return Forbid();
        }

        // PUT /api/users/me
        // Updates only the logged-in user's own profile.
[HttpPut]
public async Task<IActionResult> UpdateProfile(
 UserProfileDto request)
{
string? userId =User.FindFirstValue(ClaimTypes.NameIdentifier);

string? role =User.FindFirstValue(ClaimTypes.Role);

if (!int.TryParse(userId, out int id))
{
return Unauthorized();
}

            if (role == "Organiser")
            {
                var organiser = await _context.EventOrganisers
                    .FindAsync(id);

                if (organiser == null)
                {
                    return NotFound();
                }

                organiser.firstName = request.firstName;
                organiser.lastName = request.lastName;
                organiser.email = request.email;

                await _context.SaveChangesAsync();

                return NoContent();
            }

            if (role == "Participant")
            {
                var participant = await _context.Participants
                    .FindAsync(id);

                if (participant == null)
                {
                    return NotFound();
                }

                participant.firstName = request.firstName;
                participant.lastName = request.lastName;
                participant.email = request.email;

                await _context.SaveChangesAsync();

                return NoContent();
            }

            return Forbid();
        }
    }
}
