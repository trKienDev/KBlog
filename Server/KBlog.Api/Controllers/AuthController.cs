using KBlog.Application.DTOs.Auth;
using KBlog.Domain.Entities;
using KBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace KBlog.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly UserManager<User> _userManager;
		private readonly IConfiguration _configuration;
		private readonly ApplicationDbContext _context;

		public AuthController(
			UserManager<User> userManager, 
			IConfiguration configuration,
			ApplicationDbContext context
		)	
		{
			_userManager = userManager;
			_configuration = configuration;
			_context = context;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterDto registerDto) {
			var userByEmail = await _userManager.FindByEmailAsync(registerDto.Email);
			if (userByEmail != null) {
				return BadRequest("Email alreay exists");
			}

			var userByName = await _userManager.FindByNameAsync(registerDto.UserName);
			if (userByName != null)
			{
				return BadRequest("Username already exists.");
			}

			var newUser = new User
			{
				UserName = registerDto.UserName,
				Email = registerDto.Email,
				RegisteredAt = DateTime.UtcNow,
			};

			// Sử dụng UserManager để tạo user mới với mật khẩu đã cho
			// UserManager sẽ tự động băm (hash) mật khẩu trước khi lưu
			var result = await _userManager.CreateAsync(newUser, registerDto.Password);
			if (result.Succeeded)
			{
				// gán vai trò "Reader" cho user mới đăng ký
				await _userManager.AddToRoleAsync(newUser, "Reader");
				return Ok( new { Message = "User registered successfully" });
			}
			return BadRequest(result.Errors);
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginDto loginDto) {
			// Tìm user bằng email
			var user = await _userManager.FindByEmailAsync(loginDto.Email);
			if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
			{
				return Unauthorized("Invalid credentials.");
			}

			// Lấy danh sách vai trò của người dùng
			var roles = await _userManager.GetRolesAsync(user);

			// Ghi lại lịch sử đăng nhập
			var loginHistory = new LoginHistory
			{
				UserId = user.Id,
				LoginTime = DateTime.UtcNow,
				IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
			};
			_context.LoginHistories.Add(loginHistory);
			await _context.SaveChangesAsync();	

			// Nếu xác thực thanfhc ông, tạo JWT token
			var token = GenerateJwtToken(user, roles);
			return Ok(new { token = token });
		}

		private object GenerateJwtToken(User user, IList<string> roles)
		{
			var tokenHandler = new JwtSecurityTokenHandler();

			// Lấy chuỗi bí mật từ file cấu hình
			var secretKey = _configuration["JwtSettings:Key"];
			if(string.IsNullOrEmpty(secretKey)) {
				// Ném ra một lỗi rõ ràng nếu key không được cấu hình
				// Giúp chúng ta dễ dàng phát hiện lỗi cấu hình
				throw new InvalidOperationException("JWT Key is not configured in appsettings.json under JwtSettings:Key");
			}
			var key = Encoding.UTF8.GetBytes(secretKey);

			var claims = new List<Claim>
			{
				new Claim(JwtRegisteredClaimNames.NameId, user.Id), // Id của user
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Id duy nhất cho mỗi token
			};

			// Chỉ thêm claim Email nếu user.Email không null hoặc rỗng
			if(!string.IsNullOrEmpty(user.Email)) {
				claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
			}

			// Thêm các claim vai trò vào danh sách
			foreach(var role in roles) {
				claims.Add(new Claim(ClaimTypes.Role, role));
			}

			// Lấy & kiểm tra thời hạn token
			var durationInMinutesString = _configuration["JwtSettings:DurationInMinutes"];
			if (!double.TryParse(durationInMinutesString, out var durationInMinutes))
			{
				// Ném ra một lỗi rõ ràng để dừng chương trình ngay lập tức
				// nếu cấu hình bị thiếu hoặc không hợp lệ.
				throw new InvalidOperationException("JWT DurationInMinutes is not configured or is not a valid number in appsettings.json under JwtSettings:DurationInMinutes.");
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
