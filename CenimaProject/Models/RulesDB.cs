using CinemaProject.Models;
using System.ComponentModel.DataAnnotations;

namespace CinemaProject.Models
{
    public class RulesDB
    {
        [Key]
        public int RulesID { get; set; }

        [Required]
        public string RulesName { get; set; }
    }
}
