using KBlog.DTOs;

namespace KBlog.Services.Interfaces
{
	public interface IUserService
	{
		Task RegisterUserAsync(RegisterRequest model);
		Task<UserDTO?> GetUserByEmailAsync(string email);
		Task<UserDTO?> GetUserByIdAsync(int id);
		Task<IEnumerable<UserDTO>> GetAllUsersAsync();
		Task<UserDTO> UpdateUserAsync(int id, UpdateUser updateUserDTO);
		Task DeleteUserAsync(int id);
	}
}
