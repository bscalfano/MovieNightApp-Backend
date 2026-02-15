namespace MovieNightApp.Models
{
    public class UserFollow
    {
        public int Id { get; set; }
        public string FollowerId { get; set; } = string.Empty; // User who is following
        public string FollowingId { get; set; } = string.Empty; // User being followed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ApplicationUser Follower { get; set; } = null!;
        public ApplicationUser Following { get; set; } = null!;
    }
}