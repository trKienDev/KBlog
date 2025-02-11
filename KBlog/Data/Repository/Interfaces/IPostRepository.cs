using KBlog.Models;

namespace KBlog.Data.Repository.Interfaces
{
	public interface IPostRepository
	{
		Task<Post> CreatePostAsync(Post post);
		Task<IEnumerable<Post>> GetAllPostsAsync(int page, int pageSize);
		Task<Post?> GetPostByIdAsync(int id);
		Task<Post?> UpdatePostAsync(Post post);
		Task<bool> DeletePostAsync(Post post);
	}
}
