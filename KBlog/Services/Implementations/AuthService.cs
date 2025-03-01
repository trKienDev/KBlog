using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using KBlog.Models;
using KBlog.Services.Interfaces;
using KBlog.DTOs;
using System.Security.Cryptography;

namespace KBlog.Services.Implementations
{
	public class AuthService : IAuthService
	{
		private readonly IConfiguration _config;
		public AuthService(IConfiguration config)
		{
			_config = config;
		}

		public string GenerateJwtToken(User user)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			string? secretKey = _config["Jwt:Secret"];
			if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
			{
				throw new InvalidOperationException("JWT Secret Key is too short or not configured properly.");
			}
			var key = Encoding.UTF8.GetBytes(secretKey);

			var claims = new List<Claim> {
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Email, user.Email),
				new Claim(ClaimTypes.Name, user.Name),
				new Claim(ClaimTypes.Role, user.Role),
			};

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(claims),
				Expires = DateTime.UtcNow.AddMinutes(15),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};

			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}

		public string GenerateRefreshToken() {
			var randomNumber = new byte[32];
			using var rng = RandomNumberGenerator.Create();
			rng.GetBytes(randomNumber);
			return Convert.ToBase64String(randomNumber);
		}
		public int GetUserIdFromClaims(ClaimsPrincipal user) {
			return int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
		}
	}
}
