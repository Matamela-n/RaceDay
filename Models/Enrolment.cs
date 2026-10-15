using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RACEDAY_API_PROJECT.Models
{
    public class Enrolment
    {
[Key] //this represents the primary key
public int enrolmentID { get; set; }

[Required]
public int participantID { get; set; }

[ForeignKey("participantID")]
 public Participant? participant { get; set; } = null!;


[Required]
public int categoryID { get; set; }

[ForeignKey("categoryID")] 
public Category? category { get; set; } = null!;

[Required]
public DateTime enrolmentDate { get; set; }

 [Required]
  public string status { get; set; } = "Pending";

public Result? result { get; set; }
    }
}

