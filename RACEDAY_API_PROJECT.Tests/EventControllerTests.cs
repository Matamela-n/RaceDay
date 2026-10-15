
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
    public class EventControllerTests
    {
        private RaceDayDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        private EventController CreateController(
            RaceDayDbContext context, int organiserID)
        {
            var controller = new EventController(context);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, organiserID.ToString()),
                new Claim(ClaimTypes.Role, "Organiser")
            };

            var identity = new ClaimsIdentity(claims, "Test");
            var user = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };

            return controller;
        }

        private Event CreateTestEvent(int organiserID)
        {
            return new Event
            {
                eventName = "Test Race",
                eventDate = new DateTime(2026, 11, 20),
                location = "Johannesburg",
                eventType = "Road Race",
                description = "Unit test event",
                organiserID = organiserID
            };
        }

        [Fact]
        public async Task CreateEvent_ValidEvent_ReturnsCreated()
        {
            using var context = CreateContext();
            var controller = CreateController(context, 10);
            var newEvent = CreateTestEvent(99);

            var result = await controller.CreateEvent(newEvent);

            Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(10, newEvent.organiserID);
            Assert.Equal(1, await context.Events.CountAsync());
        }

        [Fact]
        public async Task GetEvent_ExistingEvent_ReturnsEvent()
        {
            using var context = CreateContext();
            var raceEvent = CreateTestEvent(10);

            context.Events.Add(raceEvent);
            await context.SaveChangesAsync();

            var controller = CreateController(context, 10);

            var result = await controller.GetEvent(raceEvent.eventID);

            Assert.NotNull(result.Value);
            Assert.Equal("Test Race", result.Value.eventName);
        }

        [Fact]
        public async Task UpdateEvent_EventOwnedByAnotherOrganiser_ReturnsForbid()
        {
            using var context = CreateContext();
            var raceEvent = CreateTestEvent(20);

            context.Events.Add(raceEvent);
            await context.SaveChangesAsync();

            var controller = CreateController(context, 10);

            var updatedEvent = CreateTestEvent(10);
            updatedEvent.eventName = "Updated Race";

            var result = await controller.UpdateEvent(
                raceEvent.eventID, updatedEvent);

            Assert.IsType<ForbidResult>(result);
            Assert.Equal("Test Race", raceEvent.eventName);
        }

        [Fact]
        public async Task UpdateEvent_EventDoesNotExist_ReturnsNotFound()
        {
            using var context = CreateContext();
            var controller = CreateController(context, 10);

            var result = await controller.UpdateEvent(
                999, CreateTestEvent(10));

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
