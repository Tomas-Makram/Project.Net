using System.ComponentModel.DataAnnotations;

namespace CinemaProject.Models
{
    public class UsersDB
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string Rules { get; set; }
    }
}
