using KBlog.Models;

namespace KBlog.Data.Repository.Interfaces
{
	public interface IUserRepository
	{
		Task<User?> GetUserByEmailAsync(string email);
		Task<User?> GetUserByIdAsync(int id);
		Task<IEnumerable<User>> GetAllUsersAsync();
		Task DeleteUserAsync(int id);
		Task AddUserAsync(User user);
		Task SaveChangesAsync();
	}
}
