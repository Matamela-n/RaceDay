using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RACEDAY_API_PROJECT.Models
{
    public class EventOrganiser
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

        // Organiser profile picture.
        public string? profileImagePath { get; set; }

        public ICollection<Event> Events { get; set; }
            = new List<Event>();
    }
}

