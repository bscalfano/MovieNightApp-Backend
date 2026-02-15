namespace MovieNightApp.Models
{
    public class MovieNightAttendee
    {
        public int Id { get; set; }
        public int MovieNightId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime RsvpedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public MovieNight MovieNight { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}