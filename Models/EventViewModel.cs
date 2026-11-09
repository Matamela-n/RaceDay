using System.Text.Json.Serialization;

namespace RaceDay_Web.Models;

public class EventViewModel
{
    [JsonPropertyName("eventID")]
    public int EventID { get; set; }

    [JsonPropertyName("eventName")]
    public string EventName { get; set; } = string.Empty;

    [JsonPropertyName("eventDate")]
    public DateTime EventDate { get; set; }

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("eventType")]
    public string EventType { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("organiserID")]
    public int OrganiserID { get; set; }
    [JsonPropertyName("bannerImagePath")]
    public string? BannerImagePath { get; set; }

    [JsonPropertyName("categories")]
    public List<EventCategoryViewModel> Categories { get; set; } = new();
}

public class EventCategoryViewModel
{
    [JsonPropertyName("categoryID")]
    public int CategoryID { get; set; }

    [JsonPropertyName("categoryName")]
    public string CategoryName { get; set; } = string.Empty;

    [JsonPropertyName("distance")]
    public double Distance { get; set; }
}
