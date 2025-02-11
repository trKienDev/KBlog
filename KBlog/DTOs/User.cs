using System.ComponentModel.DataAnnotations;

namespace KBlog.DTOs
{
	public class UserDTO
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Password_hash { get; set; } = string.Empty;
		public string Role { get; set; } = string.Empty ;
	}

	public class RegisterRequest
	{
		[Required]
		public string UserName { get; set; } = string.Empty;

		[Required, EmailAddress]
		public string Email { get; set; } = string.Empty;

		[Required, MinLength(6)]
		public string Password { get; set; } = string.Empty;
	}

	public class LoginRequest
	{
		[Required, EmailAddress]
		public string? Email { get; set; }

		[Required]
		[MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
		public string? Password { get; set; }
	}

	public class UpdateUser
	{
		public string? UserName { get; set; }
		public string? Email { get; set; }
	}
}
