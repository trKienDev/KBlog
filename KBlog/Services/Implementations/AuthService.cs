using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using KBlog.Models;
using KBlog.Services.Interfaces;

namespace KBlog.Services.Implementations
{
	public class AuthService : IAuthService
	{
		private readonly IConfiguration _config;
		public AuthService(IConfiguration config)
		{
			_config = config;
		}

		public string GenerateJwtToken(User user, string email, string name)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			string? secretKey = _config["Jwt:Secret"];
			if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
			{
				throw new InvalidOperationException("JWT Secret Key is too short or not configured properly.");
			}
			var key = Encoding.UTF8.GetBytes(secretKey);

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new[] {
					new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
					new Claim("name", name),
					new Claim("email", email),
				}),
				Expires = DateTime.UtcNow.AddHours(2),
				SigningCredentials = new SigningCredentials(
					new SymmetricSecurityKey(key),
					SecurityAlgorithms.HmacSha256Signature
				)
			};

			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}
	}
}
