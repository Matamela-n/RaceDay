using System.ComponentModel.DataAnnotations;

namespace RACEDAY_API_PROJECT.Models
{
public class LoginDto
{
[Required]
[EmailAddress]
public string email { get; set; } = string.Empty;

[Required]
public string password { get; set; } = string.Empty;

[Required]
public string role { get; set; } = string.Empty;
}
}
