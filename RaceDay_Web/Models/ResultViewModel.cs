
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RaceDay_Web.Models;

public class ResultViewModel
{
    [JsonPropertyName("resultID")]
    public int ResultID { get; set; }

    [Required(ErrorMessage = "Please select an enrolment.")]
    [JsonPropertyName("enrolmentID")]
    public int EnrolmentID { get; set; }

    [Required(ErrorMessage = "Finish time is required.")]
    [Display(Name = "Finish Time")]
    [JsonPropertyName("finishTime")]
    public TimeSpan FinishTime { get; set; }

    [Required(ErrorMessage = "Finishing position is required.")]
    [Range(1, 100000, ErrorMessage = "Position must be at least 1.")]
    [Display(Name = "Finishing Position")]
    [JsonPropertyName("position")]
    public int Position { get; set; }

    // Additional properties used by the MVC website.
    [JsonIgnore]
    public int EventID { get; set; }

    [JsonIgnore]
    public string EventName { get; set; } = string.Empty;

    [JsonIgnore]
    public List<EnrolmentViewModel> Enrolments { get; set; } = new();
}
