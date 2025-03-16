using System.ComponentModel.DataAnnotations;

namespace CinemaProject.Models
{
    public class MoviesDB
    {
        [Key]
        public int MovieID { get; set; }

        [Required]
        public string MovieName { get; set; }

        [Required]
        public string MovieGenre { get; set; }

        [Required]
        public string MovieDuration { get; set; }

        [Required]
        public string MovieDescription { get; set; }

        [Required]
        public string MoviePosterURL { get; set; }

        [Required]
        public string MovieLaguage { get; set; }

        [Required]
        public string MovieSubTitle { get; set; }
    }
}
