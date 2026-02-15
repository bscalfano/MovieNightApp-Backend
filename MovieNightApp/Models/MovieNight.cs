namespace MovieNightApp.Models
{
    public class MovieNight
    {
        public int Id { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public string? Notes { get; set; }
        public string? ImageUrl { get; set; }
        public string? Genre { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key to user
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        // Attendees
        public ICollection<MovieNightAttendee> Attendees { get; set; } = new List<MovieNightAttendee>();
    }
}