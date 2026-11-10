
namespace RaceDay_Web.Models;

public class OrganiserResultsViewModel
{
    public int EventID { get; set; }

    public string EventName { get; set; } = string.Empty;

    public List<OrganiserResultItemViewModel> Results { get; set; } = new();

    public List<EnrolmentViewModel> Enrolments { get; set; } = new();
}

public class OrganiserResultItemViewModel
{
    public int ResultID { get; set; }

    public int EnrolmentID { get; set; }

    public TimeSpan FinishTime { get; set; }

    public int Position { get; set; }

    public string ParticipantName { get; set; } = "Unknown participant";

    public string CategoryName { get; set; } = "Unknown category";
}
