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
        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
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
                var result = await _commentService.CreateCommentAsync(commentRequest);
                return CreatedAtAction(nameof(GetCommentById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
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
