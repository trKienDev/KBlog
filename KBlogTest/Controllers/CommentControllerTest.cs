using KBlog.Controllers;
using KBlog.Data;
using KBlog.Data.Repository.Interfaces;
using KBlog.DTOs;
using KBlog.Services.Implementations;
using KBlog.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using KBlog.Models;
using Microsoft.VisualBasic;

namespace KBlogTest.Controllers
{
	public class CommentControllerTest
	{
		private readonly Mock<ICommentService> _mockCommentService;
		private readonly CommentController _controller;

		public CommentControllerTest() {
			_mockCommentService = new Mock<ICommentService>();
			_controller = new CommentController(this._mockCommentService.Object);
		}

		private void SetupUser(int userId) {
			var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
				new Claim(ClaimTypes.NameIdentifier, userId.ToString())
			}, "mock"));

			_controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext { User = user }
			};
		}

		[Fact]
		public async Task CreateComment_Returns_CreatedAtActionResult() {
			int postId = 1, userId = 5;
			var commentDTO = new CreateCommentDTO { Content = "New Comment" };
			var expectedComment = new CommentDTO { Id = 1, PostId = postId, UserId = userId, Content = "New Comment", CreatedAt = DateTime.UtcNow };

			_mockCommentService.Setup(service => service.CreateCommentAsync(postId, userId, commentDTO))
								.ReturnsAsync(expectedComment);
			SetupUser(userId);

			var result = await _controller.CreateComment(postId, commentDTO);

			var actionResult = Assert.IsType<CreatedAtActionResult>(result);	
			var returnValue = Assert.IsType<CommentDTO>(actionResult.Value); 
			Assert.Equal(expectedComment.Content, returnValue.Content);
		}

		[Fact]
		public async Task GetCommentByPost_Returns_OkResult_With_COmments() {
			int postId = 1;
			var fakeComments = new List<CommentDTO> {
				new CommentDTO { Id = 1, PostId = postId, UserId = 5, Content = "Comment 1", CreatedAt = DateTime.UtcNow },
				new CommentDTO { Id = 2, PostId = postId, UserId = 6, Content = "Comment 2", CreatedAt = DateTime.UtcNow }
			};

			_mockCommentService.Setup(service => service.GetCommentByPostAsync(postId))
								.ReturnsAsync(fakeComments);

			var result = await _controller.GetCommentsByPost(postId);

			var okResult = Assert.IsType<OkObjectResult>(result);
			var resturnValue = Assert.IsType<List<CommentDTO>>(okResult.Value);
			Assert.Equal(2, resturnValue.Count);
		}

		[Fact]
		public async Task UpdateComment_Returns_OkResult_When_Successful() {
			int commentId = 1, userId = 5;
			var updateCommentDTO = new UpdateCommentDTO { Content = "Updated Comment" };
			var updatedComment = new CommentDTO { Id = commentId, PostId = 1, UserId = userId, Content = "Updated Comment", UpdatedAt = DateTime.UtcNow };

			_mockCommentService.Setup(service => service.UpdateCommentAsync(commentId, userId, updateCommentDTO))
								.ReturnsAsync(updatedComment);
			SetupUser(userId);

			var result = await _controller.UpdateComment(commentId, updateCommentDTO);

			var okResult = Assert.IsType<OkObjectResult>(result);
			var returnValue = Assert.IsType<CommentDTO>(okResult.Value);
			Assert.Equal("Updated Comment", returnValue.Content);				
		}

		[Fact]
		public async Task UpdateComment_Returns_NotFound_When_Comment_Does_Not_Exist() {
			int commentId = 1, userId = 5;
			var updateCommentDTO = new UpdateCommentDTO { Content = "Updated Comment" };
			_mockCommentService.Setup(service => service.UpdateCommentAsync(commentId, userId, updateCommentDTO))
								.ReturnsAsync((CommentDTO?)null);

			SetupUser (userId);

			var result = await _controller.UpdateComment(commentId, updateCommentDTO);
			var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
			Assert.Equal($"Comment with ID {commentId} was not found.", notFoundResult.Value);
		}

		[Fact]
		public async Task DeleteComment_Returns_NoContent_When_Successful() {
			int commentId = 1, userId = 5;
			_mockCommentService.Setup(service => service.DeleteCommentAsync(commentId, userId))
								.ReturnsAsync(true);
			SetupUser (userId);

			var result = await _controller.DeleteComment(commentId);
			Assert.IsType<NoContentResult>(result);
		}

		[Fact]
		public async Task DeleteComment_Returns_NotFound_When_Comment_DoesNotExist() {
			int commentId = 1, userId = 5;

			_mockCommentService.Setup(service => service.DeleteCommentAsync (commentId, userId))	
								.ReturnsAsync(false);		
			SetupUser (userId);

			var result = await _controller.DeleteComment(commentId);

			var notFoundResult = Assert.IsType<NotFoundObjectResult> (result);
			Assert.Equal($"Comment with ID {commentId} was not deleted.", notFoundResult.Value);
		}

	}
}
