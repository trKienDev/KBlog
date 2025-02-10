using KBlog.Data;
using KBlog.DTOs;
using KBlog.Models;
using KBlog.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace KBlog.Services.Implementations
{
	public class PostService : IPostService
	{
		public readonly KBlogDbContext _dbContext;
		public PostService(KBlogDbContext dbContext) { _dbContext = dbContext; }

		// Cần sửa logic để lấy tên user
		public async Task<PostDTO> CreatePostAsync(CreatePostDTO postDTO){
			var post = new Post {
				Title = postDTO.Title,
				Content = postDTO.Content,
				UserId = postDTO.userId,
				CreatedAt = DateTime.UtcNow,
			};

			_dbContext.Posts.Add(post);
			await _dbContext.SaveChangesAsync();

			return new PostDTO
			{
				Id = post.Id,
				Title = post.Title,
				Content = post.Content,
				CreatedAt = post.CreatedAt,
				UserId = post.UserId,
			};
		}

		public async Task<IEnumerable<PostDTO>> GetAllPostAsync(int page) {
			int pageSize = 10;
			var posts = await _dbContext.Posts
						.Skip((page - 1) * pageSize)
						.Take(pageSize)
						.Select(post => new PostDTO
						{
							Id = post.Id,
							Title = post.Title,
							Content = post.Content,
							CreatedAt = post.CreatedAt,
						}).ToListAsync();
			
			return posts;
		}

		public async Task<PostDTO?> GetPostByIdAsync(int id) {
			var post = await _dbContext.Posts.FirstOrDefaultAsync(p => p.Id == id);

			if(post == null) {
				return null;
			}

			return new PostDTO
			{
				Id = post.Id,
				Title = post.Title,
				Content = post.Content,
				CreatedAt = post.CreatedAt,
				UserId = post.UserId,
			};
		}

		public async Task<PostDTO?> UpdatePostAsync(int id, UpdatePostDTO postDTO) {
			var post = await _dbContext.Posts.FindAsync(id);
			if (post == null)
			{
				return null;
			}

			post.Title = postDTO.Title;
			post.Content = postDTO.Content;
			post.UpdatedAt = DateTime.UtcNow;

			_dbContext.Posts.Update(post);
			await _dbContext.SaveChangesAsync();

			return new PostDTO
			{
				Id = post.Id,
				Title = post.Title,
				Content = post.Content,
				UpdatedAt = post.UpdatedAt,
			};
		}

		public async Task<bool> DeletePostAsync(int id)
		{
			var post = await _dbContext.Posts.FindAsync(id);
			if (post == null)
			{
				return false;
			}

			_dbContext.Posts.Remove(post);
			await _dbContext.SaveChangesAsync();

			return true;
		}
	}
}
