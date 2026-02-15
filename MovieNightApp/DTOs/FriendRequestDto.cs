namespace MovieNightApp.DTOs
{
    public class FriendRequestDto
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string? SenderFirstName { get; set; }
        public string? SenderLastName { get; set; }
        public string? SenderProfilePictureUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}