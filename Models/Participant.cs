using System.ComponentModel.DataAnnotations;

namespace RACEDAY_API_PROJECT.Models
{
public class Participant 
{
[Key] //this identifies it as a primary key.
 public int participantID { get; set; }

 [Required]
 public string firstName { get; set; } = string.Empty;

[Required]
public string lastName { get; set; } = string.Empty;

[Required]
[EmailAddress] //this ensures that the user inputs a valid email in the correct email format.
public string email { get; set; } = string.Empty;

[Required]
public string password { get; set; } = string.Empty;

public ICollection<Enrolment> Enrolments { get; set; }= new List<Enrolment>();

    }
}

