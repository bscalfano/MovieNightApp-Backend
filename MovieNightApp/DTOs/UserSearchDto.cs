namespace MovieNightApp.DTOs
{
    public class UserSearchDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string FriendshipStatus { get; set; } = "none"; // "none", "pending_sent", "pending_received", "friends"
    }
}