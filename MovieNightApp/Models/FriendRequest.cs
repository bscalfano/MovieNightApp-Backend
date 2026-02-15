namespace MovieNightApp.Models
{
    public class FriendRequest
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = string.Empty; // User who sent the request
        public string ReceiverId { get; set; } = string.Empty; // User who receives the request
        public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AcceptedAt { get; set; }

        // Navigation properties
        public ApplicationUser Sender { get; set; } = null!;
        public ApplicationUser Receiver { get; set; } = null!;
    }

    public enum FriendRequestStatus
    {
        Pending = 0,
        Accepted = 1,
        Rejected = 2
    }
}