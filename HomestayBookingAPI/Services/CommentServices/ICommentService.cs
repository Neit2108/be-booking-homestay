using HomestayBookingAPI.DTOs.Comment;

namespace HomestayBookingAPI.Services.CommentServices
{
    public interface ICommentService
    {
        Task<CommentResponse> GetCommentByIdAsync(int id);
        Task<CommentResponse> CreateCommentAsync(CommentRequest commentRequest);
        Task<IEnumerable<CommentResponse>> GetAllCommentsAsync();
        Task<IEnumerable<CommentResponse>> GetAllCommentsByPlaceIdAsync(int placeId);
        Task UpdateRatingAsync(int placeId);
    }
}
