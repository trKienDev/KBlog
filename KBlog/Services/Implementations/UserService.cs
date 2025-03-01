using KBlog.Services.Interfaces;
using KBlog.Data.Repository;
using KBlog.DTOs;
using KBlog.Models;
using Microsoft.VisualBasic;
using KBlog.Data.Repository.Interfaces;
using KBlog.Data;
using Microsoft.EntityFrameworkCore;

namespace KBlog.Services.Implementations
{
	public class UserService : IUserService
	{
		private readonly IUserRepository _userRepository;
		private readonly KBlogDbContext _dbContext;
		public UserService(IUserRepository userRepository, KBlogDbContext dbContext)
		{
			_userRepository = userRepository;
			_dbContext = dbContext;
		}

		public async Task<User> RegisterUserAsync(RegisterRequest model)
		{
			var existingUser = await _userRepository.GetUserByEmailAsync(model.Email);
			if (existingUser != null)
			{
				throw new Exception("Email exists!");
			}

			string profileImagePath = string.Empty;
			if (model.ProfileImage != null)
			{
				var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
				if (!Directory.Exists(uploadsFolder))
				{
					Directory.CreateDirectory(uploadsFolder);
				}
				string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProfileImage.FileName;
				string filePath = Path.Combine(uploadsFolder, uniqueFileName);
				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await model.ProfileImage.CopyToAsync(stream);
				}

				profileImagePath = "/uploads/" + uniqueFileName;
			}

			var user = new User
			{
				Name = model.UserName,
				Email = model.Email,
				Password_hash = BCrypt.Net.BCrypt.HashPassword(model.Password),
				ProfileImageUrl = profileImagePath,
				EmailVerificationToken = Guid.NewGuid().ToString(), 
				IsEmailVerified = false 
			};

			await _userRepository.AddUserAsync(user);
			await _userRepository.SaveChangesAsync();

			return user;
		}

		public async Task<User?> GetUserByEmailAsync(string email)
		{
			var user = await _userRepository.GetUserByEmailAsync(email);
			if (user == null) { return null; }
			return new User
			{
				Id = user.Id,
				Name = user.Name,
				Email = email,
				Password_hash = user.Password_hash,
				ProfileImageUrl = user.ProfileImageUrl,
			};
		}

		public async Task<UserDTO?> GetUserByIdAsync(int id)
		{
			var user = await _userRepository.GetUserByIdAsync(id);
			if (user == null) { return null; };
			return new UserDTO
			{
				Id = user.Id,
				Name = user.Name,
				Email = user.Email,
			};
		}

		public async Task<IEnumerable<UserDTO>> GetAllUsersAsync() {
			var users = await _userRepository.GetAllUsersAsync();
			return users.Select(user => new UserDTO
			{
				Id = user.Id,
				Name = user.Name,
				Email = user.Email,
			});
		}

		public async Task<UserDTO> UpdateUserAsync(int id, UpdateUser updateUserDTO) {
			var user = await _userRepository.GetUserByIdAsync(id);
			if(user == null) { throw new System.Exception("User not found."); }

			if (!string.IsNullOrEmpty(updateUserDTO.UserName))
			{
				user.Name = updateUserDTO.UserName;
			}
			if(!string.IsNullOrEmpty(updateUserDTO.Email)) {
				user.Email = updateUserDTO.Email;
			}

			await _userRepository.SaveChangesAsync();
			return new UserDTO { Id = user.Id, Name = user.Name, Email = user.Email };
		}

		public async Task DeleteUserAsync(int id)
		{
			var user = await _userRepository.GetUserByIdAsync(id);
			if (user == null)
			{
				throw new System.Exception("User not found.");
			}
			await _userRepository.DeleteUserAsync(id);
			await _userRepository.SaveChangesAsync();
		}

		//public async Task<UserDTO?> GetUserByRefreshTokenAsync (string refreshToken) {
		//	return await _dbContext.Users.FirstOrDefaultAsync(u => u.Re)
		//} 
	}
}
