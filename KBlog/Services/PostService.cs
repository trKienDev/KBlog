
using KBlog.Data.Repository;
using KBlog.DTOs;
using KBlog.Models;
using KBlog.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace KBlog.Services
{
	public class PostService : IPostService
	{
		public readonly IPostRepository _postRepository;
		public PostService(IPostRepository postRepository)
		{
			_postRepository = postRepository;
		}

		// Cần sửa logic để lấy tên user
		public async Task<PostDTO> CreatePostAsync(CreatePostDTO postDTO)
		{
			var post = new Post
			{
				Title = postDTO.Title,
				Content = postDTO.Content,
				UserId = postDTO.userId,
				CreatedAt = DateTime.UtcNow,
			};

			post = await _postRepository.CreatePostAsync(post);

			return new PostDTO
			{
				Id = post.Id,
				Title = post.Title,
				Content = post.Content,
				CreatedAt = post.CreatedAt,
				UserId = post.UserId,
			};
		}

		public async Task<IEnumerable<PostDTO>> GetAllPostAsync(int page)
		{
			int pageSize = 10;

			var posts = await _postRepository.GetAllPostsAsync(page, pageSize);

			return posts.Select(post => new PostDTO
			{
				Id = post.Id,
				Title = post.Title,
				Content = post.Content,
				CreatedAt = DateTime.UtcNow,
			});
		}

		public async Task<PostDTO?> GetPostByIdAsync(int id)
		{
			var post = await _postRepository.GetPostByIdAsync(id);

			if (post == null)
			{
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

		public async Task<PostDTO?> UpdatePostAsync(int id, UpdatePostDTO postDTO)
		{
			var post = await _postRepository.GetPostByIdAsync(id);

			if (post == null)
			{
				return null;
			}

			post.Title = postDTO.Title;
			post.Content = postDTO.Content;
			post.UpdatedAt = DateTime.UtcNow;

			post = await _postRepository.UpdatePostAsync(post);
			if (post == null)
			{
				throw new Exception("Error updating posts");
			}
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
			var post = await _postRepository.GetPostByIdAsync(id);
			if (post == null)
			{
				return false;
			}
			return await _postRepository.DeletePostAsync(post);
		}
	}
}
