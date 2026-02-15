namespace MovieNightApp.DTOs
{
    public class UserProfileDto
    {
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalMovieNights { get; set; }
        public int UpcomingMovieNights { get; set; }
    }
}