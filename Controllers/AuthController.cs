using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RACEDAY_API_PROJECT.Data;
using RACEDAY_API_PROJECT.Models;

namespace RACEDAY_API_PROJECT.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public AuthController(RaceDayDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var email = request.email.Trim();
        var normalisedRole = request.role.Trim();

        if (!string.Equals(normalisedRole, "Participant", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(normalisedRole, "Organiser", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Role must be Participant or Organiser.");
        }

        var participantExists = await _context.Participants
            .AnyAsync(p => p.email.ToLower() == email.ToLower());

        var organiserExists = await _context.EventOrganisers
            .AnyAsync(o => o.email.ToLower() == email.ToLower());

        if (participantExists || organiserExists)
        {
            return BadRequest("Email is already registered.");
        }

        if (string.Equals(normalisedRole, "Participant", StringComparison.OrdinalIgnoreCase))
        {
            var participant = new Participant
            {
                firstName = request.firstName,
                lastName = request.lastName,
                email = email,
                password = BCrypt.Net.BCrypt.HashPassword(request.password)
            };

            _context.Participants.Add(participant);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Registration successful.",
                userId = participant.participantID,
                role = "Participant"
            });
        }

        var organiser = new EventOrganiser
        {
            firstName = request.firstName,
            lastName = request.lastName,
            email = email,
            password = BCrypt.Net.BCrypt.HashPassword(request.password)
        };

        _context.EventOrganisers.Add(organiser);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Registration successful.",
            userId = organiser.organiserID,
            role = "Organiser"
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var email = request.email.Trim();
        var normalisedRole = request.role.Trim();

        if (string.Equals(normalisedRole, "Participant", StringComparison.OrdinalIgnoreCase))
        {
            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.email.ToLower() == email.ToLower());

            if (participant == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.password, participant.password))
            {
                return Unauthorized("Invalid email or password.");
            }

            return Ok(new
            {
                userID = participant.participantID,
                email = participant.email,
                role = "Participant"
            });
        }

        if (string.Equals(normalisedRole, "Organiser", StringComparison.OrdinalIgnoreCase))
        {
            var organiser = await _context.EventOrganisers
                .FirstOrDefaultAsync(o => o.email.ToLower() == email.ToLower());

            if (organiser == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.password, organiser.password))
            {
                return Unauthorized("Invalid email or password.");
            }

            return Ok(new
            {
                userID = organiser.organiserID,
                email = organiser.email,
                role = "Organiser"
            });
        }

        return BadRequest("Role must be Participant or Organiser.");
    }
}
