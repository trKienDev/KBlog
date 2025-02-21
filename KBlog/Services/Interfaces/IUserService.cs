using KBlog.DTOs;
using KBlog.Models;

namespace KBlog.Services.Interfaces
{
	public interface IUserService
	{
		Task<User> RegisterUserAsync(RegisterRequest model);
		Task<UserDTO?> GetUserByEmailAsync(string email);
		Task<UserDTO?> GetUserByIdAsync(int id);
		Task<IEnumerable<UserDTO>> GetAllUsersAsync();
		Task<UserDTO> UpdateUserAsync(int id, UpdateUser updateUserDTO);
		Task DeleteUserAsync(int id);
	}
}
