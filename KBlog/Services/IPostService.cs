using KBlog.DTOs;

namespace KBlog.Services
{
	public interface IPostService
	{
		Task<PostDTO> CreatePostAsync(CreatePostDTO postDTO);
		Task<IEnumerable<PostDTO>> GetAllPostAsync(int page);
		Task<PostDTO?> GetPostByIdAsync(int id);
		Task<PostDTO?> UpdatePostAsync(int id, UpdatePostDTO postDTO);
		Task<bool> DeletePostAsync(int id);
	}
}
