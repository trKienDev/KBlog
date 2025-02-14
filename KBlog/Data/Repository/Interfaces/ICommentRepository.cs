using KBlog.DTOs;
using KBlog.Models;

namespace KBlog.Data.Repository.Interfaces
{
	public interface ICommentRepository
	{
		Task<Comment> CreateCommentAsync(Comment comment);
		Task<IEnumerable<Comment>> GetCommentsByPostAsync(int postId);
		Task<Comment?> GetCommentByIdAsync(int commentId);
		Task<Comment> UpdateCommentAsync(Comment comment);
		Task<bool> DeleteCommentAsync(Comment comment);
	}
}
