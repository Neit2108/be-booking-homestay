using HomestayBookingAPI.DTOs.Comment;
using HomestayBookingAPI.Services.CommentServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HomestayBookingAPI.Controllers
{
    [Route("comments")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ILogger<CommentsController> _logger;
        public CommentsController(ICommentService commentService, ILogger<CommentsController> logger)
        {
            _commentService = commentService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCommentById(int id)
        {
            try
            {
                var result = await _commentService.GetCommentByIdAsync(id);
                if (result == null)
                {
                    return NotFound(new { message = "Comment not found" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("add-comment")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> CreateComment([FromForm] CommentRequest commentRequest)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(commentRequest.Content))
                {
                    return BadRequest(new { message = "Comment content is required" });
                }

                if (commentRequest.Rating < 1 || commentRequest.Rating > 5)
                {
                    return BadRequest(new { message = "Rating must be between 1 and 5" });
                }

                // Ensure SenderId is set if not provided
                if (string.IsNullOrEmpty(commentRequest.SenderId))
                {
                    commentRequest.SenderId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(commentRequest.SenderId))
                    {
                        return BadRequest(new { message = "User ID not found" });
                    }
                }

                // commentImages can be null or empty - we've fixed the service to handle this

                var result = await _commentService.CreateCommentAsync(commentRequest);
                return CreatedAtAction(nameof(GetCommentById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("all-comments-of-place/{placeId}")]
        public async Task<IActionResult> GetAllCommentsByPlaceId(int placeId)
        {
            try
            {
                var result = await _commentService.GetAllCommentsByPlaceIdAsync(placeId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
