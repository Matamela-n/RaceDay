using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RACEDAY_API_PROJECT.Data;
using RACEDAY_API_PROJECT.Models;
using System.Security.Claims;

namespace RACEDAY_API_PROJECT.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public AuthController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            string role = request.role.Trim().ToLower();

            if (role != "organiser" && role != "participant")
            {
                return BadRequest("Role must be Organiser or Participant.");
            }

            bool organiserEmailExists = await _context.EventOrganisers
                .AnyAsync(o => o.email == request.email);

            bool participantEmailExists = await _context.Participants
                .AnyAsync(p => p.email == request.email);

            if (organiserEmailExists || participantEmailExists)
            {
                return BadRequest("An account with this email already exists.");
            }

            string hashedPassword =
                BCrypt.Net.BCrypt.HashPassword(request.password);

            if (role == "organiser")
            {
                var organiser = new EventOrganiser
                {
                    firstName = request.firstName,
                    lastName = request.lastName,
                    email = request.email,
                    password = hashedPassword
                };

                _context.EventOrganisers.Add(organiser);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    organiser.organiserID,
                    organiser.firstName,
                    organiser.lastName,
                    organiser.email,
                    role = "Organiser"
                });
            }

            var participant = new Participant
            {
                firstName = request.firstName,
                lastName = request.lastName,
                email = request.email,
                password = hashedPassword
            };

            _context.Participants.Add(participant);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                participant.participantID,
                participant.firstName,
                participant.lastName,
                participant.email,
                role = "Participant"
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            string role = request.role.Trim().ToLower();

            if (role != "organiser" && role != "participant")
            {
                return BadRequest("Role must be Organiser or Participant.");
            }

            int userID;
            string email;
            string storedPassword;
            string roleName;

            if (role == "organiser")
            {
                var organiser = await _context.EventOrganisers
                    .FirstOrDefaultAsync(o => o.email == request.email);

                if (organiser == null)
                {
                    return Unauthorized("Invalid email or password.");
                }

                userID = organiser.organiserID;
                email = organiser.email;
                storedPassword = organiser.password;
                roleName = "Organiser";
            }
            else
            {
                var participant = await _context.Participants
                    .FirstOrDefaultAsync(p => p.email == request.email);

                if (participant == null)
                {
                    return Unauthorized("Invalid email or password.");
                }

                userID = participant.participantID;
                email = participant.email;
                storedPassword = participant.password;
                roleName = "Participant";
            }

            bool passwordCorrect =
                BCrypt.Net.BCrypt.Verify(request.password, storedPassword);

            if (!passwordCorrect)
            {
                return Unauthorized("Invalid email or password.");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userID.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, roleName)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity)
            );

            return Ok(new
            {
                message = "Login successful.",
                userID = userID,
                email = email,
                role = roleName
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return Ok(new
            {
                message = "You have logged out successfully!"
            });
        }
        }
        }
