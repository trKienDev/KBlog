using KBlog.DTOs;

namespace KBlog.Services.Interfaces
{
	public interface ICommentService
	{
		Task<CommentDTO> CreateCommentAsync(int postId, int userId, CreateCommentDTO commentDTO);
		Task<IEnumerable<CommentDTO>> GetCommentByPostAsync(int postId);
		Task<CommentDTO?> UpdateCommentAsync(int commentId, int userId, UpdateCommentDTO commentDTO);
		Task<bool> DeleteCommentAsync(int commentId, int userId);
	}
}
