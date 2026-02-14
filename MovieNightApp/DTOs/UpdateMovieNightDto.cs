namespace MovieNightApp.DTOs
{
    public class UpdateMovieNightDto
    {
        public string MovieTitle { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public string? Notes { get; set; }
        public string? ImageUrl { get; set; }
        public string? Genre { get; set; }
    }
}