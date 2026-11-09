using System.Collections.Generic;

namespace RaceDay_Web.Models
{
    public class ParticipantDashboardViewModel
    {
        public string ParticipantName { get; set; } = "Participant";

        public int TotalEnrolments { get; set; }

        public int UpcomingEventsCount { get; set; }

        public int CompletedRacesCount { get; set; }

        public List<EnrolmentViewModel> UpcomingEnrolments { get; set; }
            = new List<EnrolmentViewModel>();

        public List<ParticipantResultViewModel> RecentResults { get; set; }
            = new List<ParticipantResultViewModel>();
    }
}