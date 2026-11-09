
using System.ComponentModel.DataAnnotations;

namespace RaceDay_Web.Models
{
    public class CreateEventViewModel
    {
        [Required(ErrorMessage = "Event name is required.")]
        [Display(Name = "Event Name")]
        public string EventName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event date is required.")]
        [Display(Name = "Event Date and Time")]
        public DateTime EventDate { get; set; } = DateTime.Now.AddDays(1);

        [Required(ErrorMessage = "Location is required.")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select an event type.")]
        [Display(Name = "Event Type")]
        public string EventType { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }
    }
}
