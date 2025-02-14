using KBlog.Data.Repository.Interfaces;
using KBlog.DTOs;
using KBlog.Models;
using KBlog.Services.Implementations;
using Microsoft.VisualBasic;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KBlogTest.Services
{
	public class CommentServiceTest
	{
		private readonly Mock<ICommentRepository> _mockCommentRepository;
		private readonly CommentService _commentService;

		public CommentServiceTest()
		{
			_mockCommentRepository = new Mock<ICommentRepository>();
			_commentService = new CommentService(this._mockCommentRepository.Object);
		}

		[Fact]
		public async Task CreateCommentAsync_Returns_CommentDTO()
		{
			int postID = 1, userId = 5;
			var commentDTO = new CreateCommentDTO { Content = "New Comment" };
			var comment = new Comment
			{
				Id = 1,
				PostId = postID,
				UserId = userId,
				Content = "new comment",
				CreateAt = DateTime.UtcNow,
			};

			_mockCommentRepository.Setup(repo => repo.CreateCommentAsync(It.IsAny<Comment>()))
									.ReturnsAsync(comment);

			var result = await _commentService.CreateCommentAsync(postID, userId, commentDTO);

			Assert.NotNull(result);
			Assert.Equal(comment.Id, result.Id);
			Assert.Equal(comment.Content, result.Content);
			Assert.Equal(comment.PostId, result.PostId);
			Assert.Equal(comment.UserId, result.UserId);
		}

		[Fact]
		public async Task GetCommentByPostAsync_Returns_Comments()
		{
			var postID = 1;
			var fakeComments = new List<Comment>{
				new Comment { Id = 1, PostId = postID, UserId = 5, Content = "Comment 1", CreateAt = DateTime.UtcNow },
				new Comment { Id = 2, PostId = postID, UserId = 6, Content = "Comment 2", CreateAt= DateTime.UtcNow },
			};

			_mockCommentRepository.Setup(repo => repo.GetCommentsByPostAsync(postID))
										.ReturnsAsync(fakeComments);

			var result = await _commentService.GetCommentByPostAsync(postID);

			Assert.NotNull(result);
			Assert.Equal(2, result.Count());
			Assert.Equal("Comment 1", result.First().Content);
		}

		[Fact]
		public async Task UpdateCommentAsync_Updates_Comment()
		{
			int commentId = 1, userId = 5;
			var updateCommentDTO = new UpdateCommentDTO { Content = "Updated Content" };
			var existingComment = new Comment
			{
				Id = commentId,
				PostId = 1,
				UserId = 5,
				Content = "Old Content",
				CreateAt = DateTime.UtcNow,
			};

			_mockCommentRepository.Setup(repo => repo.GetCommentByIdAsync(commentId))
									.ReturnsAsync(existingComment);
			_mockCommentRepository.Setup(repo => repo.UpdateCommentAsync(It.IsAny<Comment>()))
									.ReturnsAsync((Comment updatedComment) => updatedComment);

			var result = await _commentService.UpdateCommentAsync(commentId, userId, updateCommentDTO);

			Assert.NotNull(result);
			Assert.Equal("Updated Content", result.Content);
			Assert.Equal(commentId, result.Id);
		}

		[Fact]
		public async Task UpdateCommentAsync_ReturnNull_WhenUserNotOwner()
		{
			int commentId = 1, userId = 5, otherUserId = 10;
			var updateCommentDTO = new UpdateCommentDTO { Content = "Updated Conent" };
			var existingComment = new Comment { Id = commentId, PostId = 1, UserId = otherUserId, Content = "Old Content", CreateAt = DateTime.UtcNow };
			
			_mockCommentRepository.Setup(repo => repo.GetCommentByIdAsync(commentId))
									.ReturnsAsync(existingComment);

			var result = await _commentService.UpdateCommentAsync(commentId, userId, updateCommentDTO);

			Assert.Null(result);
		}

		[Fact]
		public async Task DeleteCommentAsync_Deletes_Comment() {
			int commentId = 1, userId = 5;
			var existingComment = new Comment { Id = commentId, PostId = 1, UserId = userId, Content = "Old Content", CreateAt = DateTime.UtcNow };
			
			_mockCommentRepository.Setup(repo => repo.GetCommentByIdAsync(commentId))
									.ReturnsAsync(existingComment);
			_mockCommentRepository.Setup(repo => repo.DeleteCommentAsync(existingComment))
									.ReturnsAsync(true);

			var result = await _commentService.DeleteCommentAsync(commentId, userId);

			Assert.True(result);
		}

		[Fact]
		public async Task DeleteCommentAsync_ReturnsFalse_WhenUserNotOwner() {
			int commentId = 1, userId = 5, otherUserId = 10;
			var existingComment = new Comment { Id = commentId, PostId = 1, UserId = otherUserId, Content = "Old Content", CreateAt = DateTime.UtcNow };

			_mockCommentRepository.Setup(repo => repo.GetCommentByIdAsync(commentId))
									.ReturnsAsync(existingComment);

			var result = await _commentService.DeleteCommentAsync(commentId, userId);

			Assert.False(result);
		}
	}
}
