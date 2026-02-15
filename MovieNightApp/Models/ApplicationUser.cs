using Microsoft.AspNetCore.Identity;

namespace MovieNightApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<MovieNight> MovieNights { get; set; } = new List<MovieNight>();

        // Friend requests sent by this user
        public ICollection<FriendRequest> SentFriendRequests { get; set; } = new List<FriendRequest>();

        // Friend requests received by this user
        public ICollection<FriendRequest> ReceivedFriendRequests { get; set; } = new List<FriendRequest>();
    }
}