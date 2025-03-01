using KBlog.DTOs;
using KBlog.Models;
using System.Security.Claims;

namespace KBlog.Services.Interfaces
{
	public interface IAuthService
	{
		string GenerateJwtToken(User user);
		int GetUserIdFromClaims(ClaimsPrincipal user);
		public string GenerateRefreshToken();
	}
}
