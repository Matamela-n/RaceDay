
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RACEDAY_API_PROJECT.Controllers;
using RACEDAY_API_PROJECT.Data;
using RACEDAY_API_PROJECT.Models;
using System.Security.Claims;
using Xunit;

namespace RACEDAY_API_PROJECT.Tests
{
    public class EnrolmentControllerTests
    {
        private RaceDayDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        private EnrolmentController CreateController(
            RaceDayDbContext context, int participantID)
        {
            var controller = new EnrolmentController(context);

            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    participantID.ToString()),
                new Claim(ClaimTypes.Role, "Participant")
            };

            var identity = new ClaimsIdentity(claims, "Test");

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            };

            return controller;
        }

        [Fact]
        public async Task CreateEnrolment_ValidCategory_ReturnsOk()
        {
            using var context = CreateContext();

            context.Categories.Add(new Category
            {
                categoryID = 1,
                categoryName = "10km",
                distance = 10,
                eventID = 1
            });

            await context.SaveChangesAsync();

            var controller = CreateController(context, 5);

            var request = new Enrolment
            {
                categoryID = 1
            };

            var result = await controller.CreateEnrolment(request);

            Assert.IsType<OkObjectResult>(result.Result);

            var savedEnrolment = await context.Enrolments
                .FirstOrDefaultAsync();

            Assert.NotNull(savedEnrolment);
            Assert.Equal(5, savedEnrolment.participantID);
            Assert.Equal(1, savedEnrolment.categoryID);
            Assert.Equal("Pending", savedEnrolment.status);
        }

        [Fact]
        public async Task CreateEnrolment_InvalidCategory_ReturnsNotFound()
        {
            using var context = CreateContext();

            var controller = CreateController(context, 5);

            var request = new Enrolment
            {
                categoryID = 999
            };

            var result = await controller.CreateEnrolment(request);

            Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(0, await context.Enrolments.CountAsync());
        }

        [Fact]
        public async Task GetEnrolment_OwnEnrolment_ReturnsOk()
        {
            using var context = CreateContext();

            context.Enrolments.Add(new Enrolment
            {
                enrolmentID = 1,
                participantID = 5,
                categoryID = 1,
                enrolmentDate = DateTime.UtcNow,
                status = "Pending"
            });

            await context.SaveChangesAsync();

            var controller = CreateController(context, 5);

            var result = await controller.GetEnrolment(1);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetEnrolment_AnotherParticipantsEnrolment_ReturnsForbid()
        {
            using var context = CreateContext();

            context.Enrolments.Add(new Enrolment
            {
                enrolmentID = 1,
                participantID = 10,
                categoryID = 1,
                enrolmentDate = DateTime.UtcNow,
                status = "Pending"
            });

            await context.SaveChangesAsync();

            var controller = CreateController(context, 5);

            var result = await controller.GetEnrolment(1);

            Assert.IsType<ForbidResult>(result);
        }
    }
}
