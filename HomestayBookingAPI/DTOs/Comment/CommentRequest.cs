namespace HomestayBookingAPI.DTOs.Comment
{
    public class CommentRequest
    {
        public string Content { get; set; }
        public int Rating { get; set; }
        public string SenderId { get; set; }
        public int PlaceId { get; set; }
        public List<IFormFile>? commentImages { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}
