
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RaceDay_Web.Models;

public class CategoryViewModel
{
    [JsonPropertyName("categoryID")]
    public int CategoryID { get; set; }

    [Required(ErrorMessage = "Category name is required.")]
    [Display(Name = "Category Name")]
    [JsonPropertyName("categoryName")]
    public string CategoryName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Distance is required.")]
    [Range(0.1, 1000, ErrorMessage = "Distance must be greater than zero.")]
    [JsonPropertyName("distance")]
    public double Distance { get; set; }

    [Required(ErrorMessage = "Please select an event.")]
    [JsonPropertyName("eventID")]
    public int EventID { get; set; }

    [JsonPropertyName("event")]
    public CategoryEventViewModel? Event { get; set; }
}

public class CategoryEventViewModel
{
    [JsonPropertyName("eventID")]
    public int EventID { get; set; }

    [JsonPropertyName("eventName")]
    public string EventName { get; set; } = string.Empty;
}

