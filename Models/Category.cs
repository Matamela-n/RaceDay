using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RACEDAY_API_PROJECT.Models
{

    public class Category
    {
        [Key]
        public int categoryID { get; set; }

        [Required]
        public string categoryName { get; set; } = string.Empty;

        [Required]
        [Precision(5, 2)] //this helps to match with the raceday database.
        public decimal distance { get; set; }

        [Required]
        public int eventID { get; set; }

        [ForeignKey("eventID")]
        public Event? Event { get; set; } = null!;
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>(); //remember one cateh=gory can have many participants enrolled in it.
    }

}
