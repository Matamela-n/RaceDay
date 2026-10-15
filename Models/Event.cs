using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RACEDAY_API_PROJECT.Models
{
    public class Event
    {
[Key]
public int eventID { get; set; }


 [Required]
public string eventName { get; set; } =string.Empty;

[Required]
public DateTime eventDate { get; set; } // the DateTime is used represnt the times and dates

[Required]
public string location { get; set; } = string.Empty;

[Required]
public string eventType { get; set; } = string.Empty;
public string? description { get; set; }

[Required]
 public int organiserID { get; set; } //this is a foreign key remember that.

[ForeignKey("organiserID")] //helps to connect the id to the event organiser model.
public EventOrganiser? Organiser { get; set; }

public ICollection<Category> Categories { get; set; } = new List<Category>();
 }
}
