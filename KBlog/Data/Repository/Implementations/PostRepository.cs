using KBlog.Data.Repository.Interfaces;
using KBlog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace KBlog.Data.Repository.Implementations
{
	public class PostRepository : IPostRepository
	{
		private readonly KBlogDbContext _dbContext;
		public PostRepository(KBlogDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<Post> CreatePostAsync(Post post)
		{
			_dbContext.Posts.Add(post);
			await _dbContext.SaveChangesAsync();
			return post;
		}

		public async Task<IEnumerable<Post>> GetAllPostsAsync(int page, int pageSize)
		{
			return await _dbContext.Posts.Skip((page - 1) * pageSize)
										.Take(pageSize)
										.ToListAsync();
		}

		public async Task<Post?> GetPostByIdAsync(int id)
		{
			return await _dbContext.Posts.FirstOrDefaultAsync(p => p.Id == id);
		}

		public async Task<Post?> UpdatePostAsync(Post post)
		{
			_dbContext.Posts.Update(post);
			await _dbContext.SaveChangesAsync();
			return post;
		}

		public async Task<bool> DeletePostAsync(Post post)
		{
			_dbContext.Posts.Remove(post);
			await _dbContext.SaveChangesAsync();
			return true;
		}
	}
}
