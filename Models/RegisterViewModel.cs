using System.ComponentModel.DataAnnotations;
namespace RaceDay_Web.Models
{
 public class RegisterViewModel
{
[Required]
 [Display(Name = "First Name")]
  public string firstName { get; set; } = string.Empty;

 [Required]
[Display(Name = "Last Name")]
public string lastName { get; set; } = string.Empty;

[Required]
[EmailAddress]
public string email { get; set; } = string.Empty;

[Required]
[StringLength(100, MinimumLength = 8)]
[DataType(DataType.Password)]
public string Password { get; set; } = string.Empty;
[Required]
public string Role { get; set; } = "Participant";

    }
}
