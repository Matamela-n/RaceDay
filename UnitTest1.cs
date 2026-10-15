
using Microsoft.EntityFrameworkCore;

using RACEDAY_API_PROJECT.Data;
using Xunit;

namespace RACEDAY_API_PROJECT.Tests
{
    public class UnitTest1
    {
        private RaceDayDbContext CreateContext()
        {
         var options = new DbContextOptionsBuilder<RaceDayDbContext>()
         .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        [Fact]
        public async Task DatabaseContext_CanBeCreated()
        {
            using var context = CreateContext();

            Assert.NotNull(context);
            Assert.NotNull(context.EventOrganisers);
            Assert.NotNull(context.Participants);
            Assert.NotNull(context.Events);
        }

        [Fact]
        public async Task DatabaseContext_CanSaveAndRetrieveParticipant()
        {
            using var context = CreateContext();

            var participant = new RACEDAY_API_PROJECT.Models.Participant
            {
                firstName = "Test",
                lastName = "Participant",
                email = "unit-test@example.com",
                password = "hashed-test-password"
            };

            context.Participants.Add(participant);
            await context.SaveChangesAsync();

            var savedParticipant = await context.Participants
                .FirstOrDefaultAsync(p => p.email == "unit-test@example.com");

            Assert.NotNull(savedParticipant);
            Assert.Equal("Test", savedParticipant.firstName);
            Assert.Equal("unit-test@example.com", savedParticipant.email);
        }
    }
}

