using Microsoft.EntityFrameworkCore;
using RACEDAY_API_PROJECT.Models;

namespace RACEDAY_API_PROJECT.Data
{

    public class RaceDayDbContext : DbContext //this connects the code to the database server. 
    {

public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
 : base(options) //this constructor will help us to get the database settings
 {
 }

 public DbSet<EventOrganiser> EventOrganisers { get; set; } //these are the tables found in the database.
public DbSet<Participant> Participants { get; set; }
 public DbSet<Event> Events { get; set; }
public DbSet<Category> Categories { get; set; }
public DbSet<Enrolment> Enrolments { get; set; }
public DbSet<Result> Results { get; set; }

    }
}