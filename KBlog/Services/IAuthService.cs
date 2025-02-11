using KBlog.Models;

namespace KBlog.Services
{
	public interface IAuthService
	{
		string GenerateJwtToken(User user, string email, string name);
	}
}
