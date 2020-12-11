using System.ComponentModel.DataAnnotations;

namespace CovidPolitical.Models
{
    public class Session
    {
        [Required]
        [StringLength(32, MinimumLength = 8)]
        public string Username { get; set; }

        [Required]
        [StringLength(32, MinimumLength = 8)]
        public string Password { get; set; }
    }
}
