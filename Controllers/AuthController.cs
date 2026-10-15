using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RACEDAY_API_PROJECT.Data;
using RACEDAY_API_PROJECT.Models;
using System.Net.Http;
using System.Security.Claims;

namespace RACEDAY_API_PROJECT.Controllers
{
   
    
    [Route("api/auth")]// This means the URL for this controller will starts with "api/auth".
    [ApiController]
    public class AuthController : ControllerBase
    {
        // _context gives this controller access to the RaceDay database.
        private readonly RaceDayDbContext _context;

       
        public AuthController(RaceDayDbContext context)
        {
            _context = context;
        }

        
        // This endpoint allows a new Organiser or Participant to register.
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            // Remove extra spaces from the role and convert it to lowercase.
            
            string role = request.role.Trim().ToLower();

            // RaceDay only allows these two account roles.
            if (role != "organiser" && role != "participant")
            {
                return BadRequest("Role must be Organiser or Participant.");
            }

            // Check whether the email already exists in the EventOrganisers table.
            bool organiserEmailExists = await _context.EventOrganisers
                .AnyAsync(o => o.email == request.email);

            // Check whether the email already exists in the Participants table.
            bool participantEmailExists = await _context.Participants
                .AnyAsync(p => p.email == request.email);

            // Doesn't allow the same email address to be registered more than once.
            if (organiserEmailExists || participantEmailExists)
            {
                return BadRequest("An account with this email already exists.");
            }

            // Hash the password using BCrypt.
            // This means the original password is NOT stored directly in the database.
            string hashedPassword =
                BCrypt.Net.BCrypt.HashPassword(request.password);

            // If the user selected the Organiser role,create an EventOrganiser.
            if (role == "organiser")
            {
                var organiser = new EventOrganiser
                {
                    firstName = request.firstName,
                    lastName = request.lastName,
                    email = request.email,
                    password = hashedPassword
                };

                // Add the organiser to the database.
                _context.EventOrganisers.Add(organiser);

                // Save the changes to SQL Server.
                await _context.SaveChangesAsync();

                // Return the organiser's information excluding the password
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

            // Add the participant to the database.
            _context.Participants.Add(participant);

            // Save the changes to SQL Server.
            await _context.SaveChangesAsync();

          
            return Ok(new
            {
                participant.participantID,
                participant.firstName,
                participant.lastName,
                participant.email,
                role = "Participant"
            });
        }//end of register
        
         
         // Allows an existing Organiser or Participant to log in.
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            
            string role = request.role.Trim().ToLower();

            // Only Organiser and Participant are valid RaceDay roles.
            if (role != "organiser" && role != "participant")
            {
                return BadRequest("Role must be Organiser or Participant.");
            }

            // These variables will store the information that we need for the authentication cookie.
            int userID;
            string email;
            string storedPassword;
            string roleName;

            // Check the EventOrganisers table if the user is trying to log in as an Organiser.
            if (role == "organiser")
            {
                var organiser = await _context.EventOrganisers
                    .FirstOrDefaultAsync(o => o.email == request.email);

                // If no organiser with this email exists, the login details are incorrect.
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

            // BCrypt compares the password entered during login with the hashed password stored in the database.
            bool passwordCorrect =
                BCrypt.Net.BCrypt.Verify(request.password, storedPassword);

            if (!passwordCorrect)
            {
                return Unauthorized("Invalid email or password.");
            }

            // Claims are pieces of information that describe the currently logged-in user.
            var claims = new List<Claim>
    {
        // Stores the user's database ID in the authentication session.
        new Claim(ClaimTypes.NameIdentifier, userID.ToString()),

        // Stores the user's email.
        new Claim(ClaimTypes.Email, email),

        // Stores whether they are an Organiser or Participant.
        new Claim(ClaimTypes.Role, roleName)
    };

            // Put the claims into an identity that uses our cookie authentication system.
            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            // SignInAsync creates the authentication cookie in the browser.
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
  await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

return Ok(new
{
message = "You have logged out successfully!"
            });
        }

    }//end of controller


}//END OF NAMESPACE
