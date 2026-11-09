using System.ComponentModel.DataAnnotations;
namespace RACEDAY_API_PROJECT.Models
{
public class UpdateStatusDto
{
[Required]
public string Status { get; set; } = string.Empty;
    }
}
