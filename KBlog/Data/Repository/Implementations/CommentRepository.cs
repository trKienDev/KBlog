using KBlog.Data.Repository.Interfaces;
using KBlog.Models;
using Microsoft.EntityFrameworkCore;

namespace KBlog.Data.Repository.Implementations
{
	public class CommentRepository : ICommentRepository
	{
		private readonly KBlogDbContext _dbContext;

		public CommentRepository(KBlogDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<Comment> CreateCommentAsync(Comment comment) {
			_dbContext.Comments.Add(comment);
			await _dbContext.SaveChangesAsync();
			return comment;
		}

		public async Task<IEnumerable<Comment>> GetCommentsByPostAsync(int postId) {
			return await _dbContext.Comments.Where(c => c.PostId == postId)
											.OrderByDescending(c => c.CreateAt)
											.ToListAsync();
		}

		public async Task<Comment?> GetCommentByIdAsync(int CommentId) {
			return await _dbContext.Comments.Include(c => c.Replies) // Nạp sẵn các bình luận con nếu có
											.FirstOrDefaultAsync(c => c.Id == CommentId);
		}

		public async Task<Comment> UpdateCommentAsync(Comment comment) {
			_dbContext.Comments.Update(comment);
			await _dbContext.SaveChangesAsync();
			return comment;	
		}

		public async Task<bool> DeleteCommentAsync(Comment comment) {
			_dbContext.Comments.Remove(comment);		
			await _dbContext.SaveChangesAsync();
			return true;
		}
	}
}
