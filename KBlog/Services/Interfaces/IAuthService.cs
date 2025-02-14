using KBlog.Models;
using System.Security.Claims;

namespace KBlog.Services.Interfaces
{
	public interface IAuthService
	{
		string GenerateJwtToken(User user, string email, string name);
		int GetUserIdFromClaims(ClaimsPrincipal user);
	}
}
