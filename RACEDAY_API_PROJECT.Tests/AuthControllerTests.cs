
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RACEDAY_API_PROJECT.Controllers;
using RACEDAY_API_PROJECT.Data;
using RACEDAY_API_PROJECT.Models;
using Xunit;

namespace RACEDAY_API_PROJECT.Tests
{
    public class AuthControllerTests
    {
        private RaceDayDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        [Fact]
        public async Task Register_ValidParticipant_ReturnsOk()
        {
            using var context = CreateContext();
            var controller = new AuthController(context);

            var request = new RegisterDto
            {
                firstName = "Test",
                lastName = "Participant",
                email = "newparticipant@example.com",
                password = "Test123!",
                role = "Participant"
            };

            var result = await controller.Register(request);

            Assert.IsType<OkObjectResult>(result);
            var saved = await context.Participants
                .FirstOrDefaultAsync(p =>
                    p.email == "newparticipant@example.com");

            Assert.NotNull(saved);
            Assert.True(BCrypt.Net.BCrypt.Verify(
                "Test123!", saved.password));
        }

        [Fact]
        public async Task Register_InvalidRole_ReturnsBadRequest()
        {
            using var context = CreateContext();
            var controller = new AuthController(context);

            var request = new RegisterDto
            {
                firstName = "Test",
                lastName = "User",
                email = "invalidrole@example.com",
                password = "Test123!",
                role = "Administrator"
            };

            var result = await controller.Register(request);

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Empty(await context.Participants.ToListAsync());
            Assert.Empty(await context.EventOrganisers.ToListAsync());
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsBadRequest()
        {
            using var context = CreateContext();

            context.Participants.Add(new Participant
            {
                firstName = "Existing",
                lastName = "Participant",
                email = "existing@example.com",
                password = BCrypt.Net.BCrypt.HashPassword("Test123!")
            });

            await context.SaveChangesAsync();

            var controller = new AuthController(context);

            var request = new RegisterDto
            {
                firstName = "Another",
                lastName = "Participant",
                email = "existing@example.com",
                password = "Test123!",
                role = "Participant"
            };

            var result = await controller.Register(request);

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(1, await context.Participants.CountAsync());
        }

        [Fact]
        public async Task Login_UnknownEmail_ReturnsUnauthorized()
        {
            using var context = CreateContext();
            var controller = new AuthController(context);

            var request = new LoginDto
            {
                email = "unknown@example.com",
                password = "Test123!",
                role = "Participant"
            };

            var result = await controller.Login(request);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Login_IncorrectPassword_ReturnsUnauthorized()
        {
            using var context = CreateContext();

            context.Participants.Add(new Participant
            {
                firstName = "Test",
                lastName = "Participant",
                email = "wrong-password@example.com",
                password = BCrypt.Net.BCrypt.HashPassword("Correct123!")
            });

            await context.SaveChangesAsync();

            var controller = new AuthController(context);

            var request = new LoginDto
            {
                email = "wrong-password@example.com",
                password = "Wrong123!",
                role = "Participant"
            };

            var result = await controller.Login(request);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

    

[Fact]
        public async Task Login_ValidCredentials_ReturnsOk()
        {
            using var context = CreateContext();

            var password = "Test123!";

            context.Participants.Add(new Participant
            {
                firstName = "Test",
                lastName = "Participant",
                email = "valid-login@example.com",
                password = BCrypt.Net.BCrypt.HashPassword(password)
            });

            await context.SaveChangesAsync();

            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

            services.AddLogging();
            services.AddAuthentication()
    .AddCookie("Cookies");

            var serviceProvider = services.BuildServiceProvider();

            var controller = new AuthController(context);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = serviceProvider
                }
            };

            var request = new LoginDto
            {
                email = "valid-login@example.com",
                password = password,
                role = "Participant"
            };

            var result = await controller.Login(request);

            Assert.IsType<OkObjectResult>(result);
        }

    }
}
