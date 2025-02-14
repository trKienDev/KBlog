using KBlog.DTOs;
using KBlog.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.Security.Claims;

namespace KBlog.Controllers
{
	[ApiController]
	[Route("api/posts/{postId}/comments")]
	public class CommentController : Controller
	{
		private readonly ICommentService _commentService;
		public CommentController(ICommentService commentService)
		{
			_commentService = commentService;
		}

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> CreateComment(int postId, [FromBody] CreateCommentDTO commentDTO)
		{
			if (commentDTO == null)
			{
				return BadRequest("Comment data cannot be null.");
			}

			var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
			var result = await _commentService.CreateCommentAsync(postId, userId, commentDTO);
			return CreatedAtAction(nameof(GetCommentsByPost), new { postId }, result);
		}

		[HttpGet]
		public async Task<IActionResult> GetCommentsByPost(int postId) {
			var comments = await _commentService.GetCommentByPostAsync(postId);	
			return Ok(comments);
		}

		[HttpPut("{commentId}")]
		[Authorize]
		public async Task<IActionResult> UpdateComment(int commentId, [FromBody] UpdateCommentDTO commentDTO) {
			if(commentDTO == null) {
				return BadRequest("Comment data cannot be null.");
			}

			var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
			var updatedComment = await _commentService.UpdateCommentAsync(commentId, userId, commentDTO);
			if(updatedComment == null) {
				return NotFound($"Comment with ID {commentId} was not found.");
			}
			return Ok(updatedComment);
		}

		[HttpDelete("{commentId}")]
		[Authorize]
		public async Task<IActionResult> DeleteComment(int commentId) {
			var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
			var success = await _commentService.DeleteCommentAsync(commentId, userId);
			if (!success)
			{
				return NotFound($"Comment with ID {commentId} was not deleted.");
			}

			return NoContent();
		}

	}
}
