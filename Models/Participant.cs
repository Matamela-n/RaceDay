using System.ComponentModel.DataAnnotations;

namespace RACEDAY_API_PROJECT.Models
{
    public class Participant
    {
        [Key]
        public int participantID { get; set; }

        [Required]
        public string firstName { get; set; } = string.Empty;

        [Required]
        public string lastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string email { get; set; } = string.Empty;

        [Required]
        public string password { get; set; } = string.Empty;

        // Stores the profile picture path.
        public string? profileImagePath { get; set; }

        public ICollection<Enrolment> Enrolments { get; set; }
            = new List<Enrolment>();
    }
}