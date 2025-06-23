using KBlog.Application.Contracts.Identity;
using KBlog.Application.Contracts.Persistence;
using KBlog.Application.DTOs.Auth;
using KBlog.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace KBlog.Application.Services
{
	public class AuthService (
		UserManager<User> userManager,
		IConfiguration configuration,
		ILoginHistoryRepository loginHistoryRepository
	) : IAuthService
	{
		private readonly UserManager<User> _userManager = userManager; 
		private readonly IConfiguration _configuration = configuration;
		private readonly ILoginHistoryRepository _loginHistoryRepository = loginHistoryRepository;
		public async Task<IdentityResult> RegisterAsync(RegisterDto registerDto)
		{
			var userByEmail = await _userManager.FindByEmailAsync(registerDto.Email);
			if (userByEmail != null)
			{
				return IdentityResult.Failed(new IdentityError { Code = "EmailInUse", Description = "Email already exists." });
			}

			var userByUsername = await _userManager.FindByNameAsync(registerDto.UserName);
			if (userByUsername != null)
			{
				return IdentityResult.Failed(new IdentityError { Code = "UsernameInUse", Description = "Username already exists." });
			}

			var newUser = new User
			{
				UserName = registerDto.UserName,
				Email = registerDto.Email,
				RegisteredAt = DateTime.UtcNow,
			};
			var result = await _userManager.CreateAsync(newUser, registerDto.Password);
			if (result.Succeeded)
			{
				await _userManager.AddToRoleAsync(newUser, "Reader");
			}
			return result;
		}

		public async Task<string?> LoginAsync(LoginDto loginDto)
		{
			var user = await _userManager.FindByEmailAsync(loginDto.Email);
			if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password)) {
				return null;
			}

			var roles = await _userManager.GetRolesAsync(user);

			var loginHistory = new LoginHistory
			{
				UserId = user.Id,
				LoginTime = DateTime.UtcNow,
				// Lưu ý: HttpContext không có sẵn ở đây, chúng ta sẽ bỏ qua IpAddress
				// hoặc cần một cách khác để truyền nó vào. Tạm thời bỏ qua để đơn giản.
				IpAddress = null
			};
			await _loginHistoryRepository.AddAsync(loginHistory);

			var token = GenerateJwtToken(user, roles);
			return token;
		}

		private string GenerateJwtToken(User user, IList<string> roles) {
			var tokenHandler = new JwtSecurityTokenHandler();
			var secretKey = _configuration["JwtSettings:Key"];
			if (string.IsNullOrEmpty(secretKey))
			{
				throw new InvalidOperationException("JWT Key is not configured...");
			}
			var key = Encoding.UTF8.GetBytes(secretKey);

			var claims = new List<Claim>
			{
				new(ClaimTypes.NameIdentifier, user.Id), // Dùng ClaimTypes.NameIdentifier
				new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			if (!string.IsNullOrEmpty(user.Email))
			{
				claims.Add(new Claim(ClaimTypes.Email, user.Email)); // Dùng ClaimTypes.Email
			}
			foreach (var role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role));
			}

			var durationInMinutesString = _configuration["JwtSettings:DurationInMinutes"];
			if (!double.TryParse(durationInMinutesString, out var durationInMinutes))
			{
				throw new InvalidOperationException("JWT Duration is not configured...");
			}

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(claims),
				Expires = DateTime.UtcNow.AddMinutes(durationInMinutes),
				Issuer = _configuration["JwtSettings:Issuer"],
				Audience = _configuration["JwtSettings:Audience"],
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
			};

			var securityToken = tokenHandler.CreateToken(tokenDescriptor);
			var token = tokenHandler.WriteToken(securityToken);
			return token;
		}
	}
}
