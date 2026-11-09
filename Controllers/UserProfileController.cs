using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RACEDAY_API_PROJECT.Data;
using RACEDAY_API_PROJECT.Models;
using System.Security.Claims;

namespace RACEDAY_API_PROJECT.Controllers
{
    [Route("api/users/me")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProfileController(
            RaceDayDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: api/users/me
        [HttpGet]
        public async Task<ActionResult<UserProfileDto>> GetProfile()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string? role = User.FindFirstValue(ClaimTypes.Role);

            if (!int.TryParse(userId, out int id))
                return Unauthorized();

            if (role == "Organiser")
            {
                var organiser = await _context.EventOrganisers.FindAsync(id);

                if (organiser == null)
                    return NotFound();

                return Ok(new UserProfileDto
                {
                    firstName = organiser.firstName,
                    lastName = organiser.lastName,
                    email = organiser.email,
                    profileImagePath = organiser.profileImagePath

                });
            }

            if (role == "Participant")
            {
                var participant = await _context.Participants.FindAsync(id);

                if (participant == null)
                    return NotFound();

                return Ok(new UserProfileDto
                {
                    firstName = participant.firstName,
                    lastName = participant.lastName,
                    email = participant.email,
                    profileImagePath = participant.profileImagePath
                });
            }

            return Forbid();
        }

        // PUT: api/users/me
        [HttpPut]
        public async Task<IActionResult> UpdateProfile(UserProfileDto request)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string? role = User.FindFirstValue(ClaimTypes.Role);

            if (!int.TryParse(userId, out int id))
                return Unauthorized();

            if (role == "Organiser")
            {
                var organiser = await _context.EventOrganisers.FindAsync(id);

                if (organiser == null)
                    return NotFound();

                organiser.firstName = request.firstName;
                organiser.lastName = request.lastName;
                organiser.email = request.email;

                await _context.SaveChangesAsync();
                return NoContent();
            }

            if (role == "Participant")
            {
                var participant = await _context.Participants.FindAsync(id);

                if (participant == null)
                    return NotFound();

                participant.firstName = request.firstName;
                participant.lastName = request.lastName;
                participant.email = request.email;

                await _context.SaveChangesAsync();
                return NoContent();
            }

            return Forbid();
        }

[Authorize(Roles = "Participant,Organiser")]
        [HttpPost("profile-image")]
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No photo selected.");

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("Image must not exceed 5MB.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Only JPG, JPEG, PNG and WEBP images are allowed.");

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "profiles"
            );

            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{role}_{userId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);
            var relativePath = $"/uploads/profiles/{fileName}";

            if (role == "Participant")
            {
                var participant = await _context.Participants.FindAsync(userId);

                if (participant == null)
                    return NotFound("Participant not found.");

                DeleteOldProfileImage(participant.profileImagePath);

                participant.profileImagePath = relativePath;
            }
            else if (role == "Organiser")
            {
                var organiser = await _context.EventOrganisers.FindAsync(userId);

                if (organiser == null)
                    return NotFound("Organiser not found.");

                DeleteOldProfileImage(organiser.profileImagePath);

                organiser.profileImagePath = relativePath;
            }
            else
            {
                return Forbid();
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile picture uploaded successfully.",
                profileImagePath = relativePath
            });
        }

        private void DeleteOldProfileImage(string? oldImagePath)
        {
            if (string.IsNullOrWhiteSpace(oldImagePath))
                return;

            var oldFileName = Path.GetFileName(oldImagePath);
            var oldFilePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "profiles",
                oldFileName
            );

            if (System.IO.File.Exists(oldFilePath))
                System.IO.File.Delete(oldFilePath);
        }
    }
}
