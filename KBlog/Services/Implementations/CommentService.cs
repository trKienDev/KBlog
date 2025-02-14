using KBlog.Data.Repository.Interfaces;
using KBlog.DTOs;
using KBlog.Models;
using KBlog.Services.Interfaces;
using System.Runtime.InteropServices;

namespace KBlog.Services.Implementations
{
	public class CommentService : ICommentService
	{
		private readonly ICommentRepository _commentRepository;
		public CommentService(ICommentRepository commentRepository)
		{
			_commentRepository = commentRepository;
		}

		public async Task<CommentDTO> CreateCommentAsync(int postId, int userId, CreateCommentDTO commentDTO) {
			var comment = new Comment
			{
				PostId = postId,
				UserId = userId,
				Content = commentDTO.Content,
				CreateAt = DateTime.UtcNow,
			};
			comment = await _commentRepository.CreateCommentAsync(comment);

			return new CommentDTO
			{
				Id = comment.Id,
				Content = comment.Content,
				CreatedAt = comment.CreateAt,
				UserId = comment.UserId.HasValue ? comment.UserId.Value : throw new Exception("UserId không thể là null"),
				PostId = comment.PostId,
			};
		}

		public async Task<IEnumerable<CommentDTO>> GetCommentByPostAsync(int postId) {
			var comments = await _commentRepository.GetCommentsByPostAsync(postId);
			return comments.Select(c => new CommentDTO
			{
				Id = c.PostId,
				Content = c.Content,
				CreatedAt = c.CreateAt,
				UserId = c.UserId.HasValue ? c.UserId.Value : throw new Exception("UserId không thể là null"),
				PostId = c.PostId,
			});
		}

		public async Task<CommentDTO?> UpdateCommentAsync(int commentId, int userId, UpdateCommentDTO commentDTO)
		{
			var comment = await _commentRepository.GetCommentByIdAsync(commentId);
			if (comment == null || comment.UserId != userId)
			{
				return null;
			}
			comment.Content = commentDTO.Content;
			comment.UpdatedAt = DateTime.UtcNow;

			comment = await _commentRepository.UpdateCommentAsync(comment);
			return new CommentDTO
			{
				Id = comment.Id,
				Content = comment.Content,
				UpdatedAt = comment.UpdatedAt,
			};
		}

		public async Task<bool> DeleteCommentAsync(int commentId, int userId) {
			var comment = await _commentRepository.GetCommentByIdAsync(commentId);
			if(comment == null || comment.UserId != userId) { return false; }
			return await _commentRepository.DeleteCommentAsync(comment);
		}
	}
}
