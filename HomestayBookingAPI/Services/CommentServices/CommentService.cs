using HomestayBookingAPI.Data;
using HomestayBookingAPI.DTOs.Comment;
using HomestayBookingAPI.DTOs.Place;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Models.Enum;
using HomestayBookingAPI.Services.ImageServices;
using Microsoft.EntityFrameworkCore;

namespace HomestayBookingAPI.Services.CommentServices
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly ILogger<CommentService> _logger;
        public CommentService(ApplicationDbContext context, IImageService imageService, ILogger<CommentService> logger)
        {
            _imageService = imageService;
            _context = context;
            _logger = logger;
        }
        public async Task<CommentResponse> CreateCommentAsync(CommentRequest commentRequest)
        {
            try
            {
                // Nếu người dùng chưa có booking ở địa điểm này không cho comment
                var booking = await _context.Bookings
                    .Where(b => b.UserId == commentRequest.SenderId && b.PlaceId == commentRequest.PlaceId)
                    .Select(b => new { b.Id, b.UserId, b.PlaceId, b.Status })
                    .FirstOrDefaultAsync();
                if (booking == null)
                {
                    _logger.LogWarning("Người dùng chưa có booking ở địa điểm này.");
                    throw new Exception("Người dùng chưa có booking ở địa điểm này.");
                }
                if(booking.Status != BookingStatus.Confirmed)
                {
                    _logger.LogWarning("Người dùng chưa hoàn thành booking ở địa điểm này.");
                    throw new Exception("Người dùng chưa hoàn thành booking ở địa điểm này.");
                }
                _logger.LogInformation("Số ảnh được chọn" + commentRequest.commentImages.Count.ToString());

                var commentImages = new List<CommentImage>();
                if (commentRequest.commentImages != null)
                {
                            foreach (var imageFile in commentRequest.commentImages)
                            {
                                var imageUrl = await _imageService.UploadImageAsync(imageFile, "comments");
                                if (imageUrl != null)
                                {
                                    commentImages.Add(new CommentImage { ImageUrl = imageUrl });
                                }
                                else
                                {
                                    _logger.LogWarning("Lỗi tải ảnh.");
                                }
                            }
                    _logger.LogInformation(commentImages.Count.ToString());
                    if (!commentImages.Any())
                    {
                        _logger.LogError("Không ảnh nào được thêm.");
                        throw new Exception("Không ảnh nào được thêm.");
                    }
                }

                var comment = new Comment
                {
                    Content = commentRequest.Content,
                    Rating = commentRequest.Rating,
                    SenderId = commentRequest.SenderId,
                    PlaceId = commentRequest.PlaceId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Images = commentImages
                };

               
                await _context.Comments.AddAsync(comment);
                await _context.SaveChangesAsync();

                await UpdateRatingAsync(commentRequest.PlaceId);

                return new CommentResponse
                {
                    Id = comment.Id,
                    Content = comment.Content,
                    Rating = comment.Rating,
                    SenderId = comment.SenderId,
                    PlaceId = comment.PlaceId,
                    CreatedAt = comment.CreatedAt,
                    UpdatedAt = comment.UpdatedAt,
                    Images = comment.Images != null
                        ? comment.Images.Select(i => new CommentImageResponse
                        {
                            Id = i.Id,
                            ImageUrl = i.ImageUrl,
                        })
                        .OrderBy(i => i.Id)
                        .ToList()
                        : new List<CommentImageResponse>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo bình luận.");
                throw new Exception("Lỗi khi tạo bình luận.", ex);
            }

        }

        public async Task<IEnumerable<CommentResponse>> GetAllCommentsAsync()
        {
            try
            {
                var comments = await _context.Comments
                    .Include(c => c.Images)
                    .ToListAsync();
                if(comments == null || !comments.Any())
                {
                    _logger.LogWarning("Không có bình luận nào.");
                    return Enumerable.Empty<CommentResponse>();
                }
                return comments.Select(c => new CommentResponse
                {
                    Id = c.Id,
                    Content = c.Content,
                    Rating = c.Rating,
                    SenderId = c.SenderId,
                    PlaceId = c.PlaceId,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    Images = c.Images != null
                        ? c.Images.Select(i => new CommentImageResponse
                        {
                            Id = i.Id,
                            ImageUrl = i.ImageUrl,
                        })
                        .OrderBy(i => i.Id)
                        .ToList()
                        : new List<CommentImageResponse>()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách bình luận.");
                throw new Exception("Lỗi khi lấy danh sách bình luận.", ex);
            }
        }

        public async Task<IEnumerable<CommentResponse>> GetAllCommentsByPlaceIdAsync(int placeId)
        {
            try
            {
                var comments = await _context.Comments
                    .Include(c => c.Images)
                    .Where(c => c.PlaceId == placeId)
                    .ToListAsync();
                if(comments == null || !comments.Any())
                {
                    _logger.LogWarning("Không có bình luận nào cho PlaceId: {PlaceId}", placeId);
                    return Enumerable.Empty<CommentResponse>();
                }
                return comments.Select(c => new CommentResponse
                {
                    Id = c.Id,
                    Content = c.Content,
                    Rating = c.Rating,
                    SenderId = c.SenderId,
                    PlaceId = c.PlaceId,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    Images = c.Images != null
                        ? c.Images.Select(i => new CommentImageResponse
                        {
                            Id = i.Id,
                            ImageUrl = i.ImageUrl,
                        })
                        .OrderBy(i => i.Id)
                        .ToList()
                        : new List<CommentImageResponse>()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách bình luận theo PlaceId.");
                throw new Exception("Lỗi khi lấy danh sách bình luận theo PlaceId.", ex);
            }
        }

        public async Task<CommentResponse> GetCommentByIdAsync(int id)
        {
            try
            {
                var comment = await _context.Comments
                    .Include(c => c.Images)
                    .FirstOrDefaultAsync(c => c.Id == id);
                if (comment == null)
                {
                    _logger.LogWarning("Không tìm thấy bình luận với Id: {Id}", id);
                    return null;
                }
                return new CommentResponse
                {
                    Id = comment.Id,
                    Content = comment.Content,
                    Rating = comment.Rating,
                    SenderId = comment.SenderId,
                    PlaceId = comment.PlaceId,
                    CreatedAt = comment.CreatedAt,
                    UpdatedAt = comment.UpdatedAt,
                    Images = comment.Images != null
                        ? comment.Images.Select(i => new CommentImageResponse
                        {
                            Id = i.Id,
                            ImageUrl = i.ImageUrl,
                        })
                        .OrderBy(i => i.Id)
                        .ToList()
                        : new List<CommentImageResponse>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy bình luận theo Id.");
                throw new Exception("Lỗi khi lấy bình luận theo Id.", ex);
            }
        }

        public async Task UpdateRatingAsync(int placeId)
        {
            var comments = await _context.Comments
                .Where(c => c.PlaceId == placeId)
                .ToListAsync();
            var place = await _context.Places
                .FirstOrDefaultAsync(p => p.Id == placeId);

            if (comments.Count > 0)
            {
                double totalRating = comments.Sum(c => c.Rating);
                place.Rating = totalRating / comments.Count;
                place.NumOfRating = comments.Count; // Cập nhật số lượng đánh giá
            }
            else
            {
                // Nếu không có comment nào, set rating về 0
                place.Rating = 0;
                place.NumOfRating = 0;
            }
            _context.Places.Update(place);
            await _context.SaveChangesAsync();
        }
    }
}
