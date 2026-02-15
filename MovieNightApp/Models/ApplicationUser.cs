using Microsoft.AspNetCore.Identity;

namespace MovieNightApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<MovieNight> MovieNights { get; set; } = new List<MovieNight>();
    }
}