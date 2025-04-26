namespace HomestayBookingAPI.DTOs.Comment
{
    public class CommentResponse
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string SenderAvatar { get; set; }
        public int PlaceId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public List<CommentImageResponse> Images { get; set; }
    }
}
