using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; //import these two libraries.


namespace RACEDAY_API_PROJECT.Models
{
public class EventOrganiser// this is a table in the RACEDAY database.
{
[Key]
public int organiserID { get; set; }

[Required]
 public string firstName { get; set; } = string.Empty;

[Required]
public string lastName { get; set; } = string.Empty;

[Required]
[EmailAddress]
public string email { get; set; } = string.Empty;

[Required]
public string password { get; set; } = string.Empty;
 public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
