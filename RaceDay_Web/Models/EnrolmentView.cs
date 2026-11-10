using System.Text.Json.Serialization;

namespace RaceDay_Web.Models;

public class EnrolmentViewModel
{
    [JsonPropertyName("enrolmentID")]
    public int EnrolmentID { get; set; }

    [JsonPropertyName("participantID")]
    public int ParticipantID { get; set; }

    [JsonPropertyName("categoryID")]
    public int CategoryID { get; set; }

    [JsonPropertyName("enrolmentDate")]
    public DateTime? EnrolmentDate { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("participant")]
    public EnrolmentParticipantViewModel? Participant { get; set; }

    [JsonPropertyName("category")]
    public EnrolmentCategoryViewModel? Category { get; set; }
}

public class EnrolmentParticipantViewModel
{
    [JsonPropertyName("participantID")]
    public int ParticipantID { get; set; }

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}

public class EnrolmentCategoryViewModel
{
    [JsonPropertyName("categoryID")]
    public int CategoryID { get; set; }

    [JsonPropertyName("categoryName")]
    public string CategoryName { get; set; } = string.Empty;

    [JsonPropertyName("distance")]
    public double Distance { get; set; }

    [JsonPropertyName("event")]
    public EnrolmentEventViewModel? Event { get; set; }
}

public class EnrolmentEventViewModel
{
    [JsonPropertyName("eventID")]
    public int EventID { get; set; }

    [JsonPropertyName("eventName")]
    public string EventName { get; set; } = string.Empty;

    [JsonPropertyName("eventDate")]
    public DateTime EventDate { get; set; }

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;
}