using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RACEDAY_API_PROJECT.Models
{
    public class Result
    {
[Key]
  public int resultID { get; set; }

[Required]
public int enrolmentID { get; set; }

 [ForeignKey("enrolmentID")]
public Enrolment? Enrolment { get; set; } = null!;

 [Required]
public TimeSpan finishTime { get; set; }

[Required]
public int position { get; set; }

}
}
