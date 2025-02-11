using KBlog.Models;

namespace KBlog.Services.Interfaces
{
	public interface IAuthService
	{
		string GenerateJwtToken(User user, string email, string name);
	}
}
