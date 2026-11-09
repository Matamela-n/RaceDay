
using System.ComponentModel.DataAnnotations;

namespace RaceDay_Web.Models
{
    public class CreateEnrolmentViewModel
    {
        [Required]
        public int categoryID { get; set; }

        public int eventID { get; set; }
    }
}
