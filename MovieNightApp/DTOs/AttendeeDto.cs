namespace MovieNightApp.DTOs
{
    public class AttendeeDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public DateTime RsvpedAt { get; set; }
    }
}