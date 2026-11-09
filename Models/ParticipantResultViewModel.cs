using System.Text.Json.Serialization;
 namespace RaceDay_Web.Models;
public class ParticipantResultViewModel
{
    [JsonPropertyName("resultID")]
    public int ResultID { get; set; }

    [JsonPropertyName("enrolmentID")]
    public int EnrolmentID { get; set; }

    [JsonPropertyName("finishTime")]
    public TimeSpan FinishTime { get; set; }

    [JsonPropertyName("position")]
    public int Position { get; set; }

    public string EventName { get; set; } = "Race event";
    public string CategoryName { get; set; } = "Race category";
}

