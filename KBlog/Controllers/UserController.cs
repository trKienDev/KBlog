using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Net.WebSockets;
using KBlog.Models;
using KBlog.DTOs;
using KBlog.Data;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using KBlog.Services.Interfaces;
using KBlog.Services;
using System.Runtime.InteropServices;
using KBlog.Data.Repository.Implementations;
using Azure.Core;

namespace KBlog.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly KBlogDbContext _dbContext;
		private readonly IAuthService _authService;
		private readonly IUserService _userService;
		private readonly IEmailService _emailService;
		private readonly ILogger<UserController> _logger;

		public UserController(	KBlogDbContext context, IAuthService authService, IUserService userService, IEmailService emailService, ILogger<UserController> logger) {
			_dbContext = context;
			_authService = authService;
			_userService = userService;
			_emailService = emailService;
			_logger = logger;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromForm] RegisterRequest model)
		{
			try
			{
				_logger.LogInformation("Starting user registration process for email: {Email}", model.Email);

				var user = await _userService.RegisterUserAsync(model);
				if (user == null)
				{
					_logger.LogWarning("User registration failed for email: {Email}", model.Email);
					return BadRequest("User registration failed.");
				}

				if (string.IsNullOrEmpty(user.EmailVerificationToken))
				{
					return BadRequest("Email verification token is missing.");
				}
				var emailBody = await _emailService.BuildVerificationEmailAsync(user.Email, user.EmailVerificationToken);
				await _emailService.SendEmailAsync(user.Email, "Xác thực tài khoản KBlog", emailBody);

				return Ok(new { message = "Register Successfully. Please check your email to verify your account.", success = true });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occured during the registration process for email: {Email}", model.Email);
				return BadRequest(new { error = ex.Message });
			}
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequest model)
		{
			var user = await _userService.GetUserByEmailAsync(model.Email);
			if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password_hash))
			{
				return Unauthorized("Invalid email or password");
			}

			var accessToken = _authService.GenerateJwtToken(user);

			var refreshTokenStr = _authService.GenerateRefreshToken();
			var refreshTokenEntity = new RefreshToken
			{
				Token = refreshTokenStr,
				ExpiryTime = DateTime.UtcNow.AddDays(7),
				UserId = user.Id,
			};

			_dbContext.RefreshTokens.Add(refreshTokenEntity);
			await _dbContext.SaveChangesAsync();

			Response.Cookies.Append("refreshToken", refreshTokenStr, new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.Strict,
				Expires = DateTime.UtcNow.AddDays(7)
			});

			return Ok(new { accessToken });

		}

		[Authorize]
		[HttpGet("profile")]
		public async Task<IActionResult> GetProfile() {
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId)) {
				return Unauthorized("User ID not found in token.");
			}

			var user = await _userService.GetUserByIdAsync(int.Parse(userId));
			if (user == null)
			{
				return NotFound("User not found.");
			}
			return Ok(new { 
				user.Id,
				user.Email,
				user.Name
			});
		}

		[Authorize]
		[HttpGet("all")]
		public async Task<IActionResult> GetAllUsers()
		{
			if (User?.Identity == null || !User.Identity.IsAuthenticated)
			{
				return Unauthorized();
			}

			var users = await _userService.GetAllUsersAsync();
			return Ok(users);
		}

		[Authorize]
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUser? updateUserDTO) {
			if (updateUserDTO == null)
			{
				return BadRequest("Name or Email must be provided.");
			}

			var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(currentUserId))
			{
				return Unauthorized("User ID not found in token.");
			}
			var user = await _userService.GetUserByIdAsync(id);
			if (user == null)
			{
				return NotFound("User not found.");
			}
			if (currentUserId != user.Id.ToString())
			{
				return Forbid("You are not allowed to update this user.");
			}

			var updateUser = await _userService.UpdateUserAsync(id, updateUserDTO);
			return Ok(new
			{
				updateUser.Id,
				updateUser.Name,
				updateUser.Email,
			});
		}

		[Authorize]
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteUser(int id) {
			var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(currentUserId))
			{
				return Unauthorized("User ID not found in token.");
			}

			var user = await _userService.GetUserByIdAsync(id);
			if(user == null) {
				return NotFound("User not found.");
			}

			var isAdmin = User.IsInRole("Admin");
			if (!isAdmin && currentUserId != user.Id.ToString())
			{
				return Forbid("You are not allowed to delete this user.");
			}

			await _userService.DeleteUserAsync(id);
			return NoContent();
		}

		[HttpPost("refresh-token")]
		public async Task<IActionResult> RefreshToken()
		{
			var refreshToken = Request.Cookies["refreshToken"];
			if (refreshToken == null)
			{
				return Unauthorized("No refresh token");
			}

			var refreshRecord = await _dbContext.RefreshTokens.Include(r => r.User)
											.FirstOrDefaultAsync(r => r.Token == refreshToken);
			if (refreshRecord == null || refreshRecord.ExpiryTime < DateTime.UtcNow)
			{
				return Unauthorized("Invalid or expired refresh token");
			}

			var user = refreshRecord.User;
			var newAccessToken = _authService.GenerateJwtToken(user);

			var newRefreshTokenStr = _authService.GenerateRefreshToken();
			var newRefreshTokenEntity = new RefreshToken
			{
				Token = newRefreshTokenStr,
				ExpiryTime = DateTime.UtcNow.AddDays(7),
				UserId = user.Id,
			};

			_dbContext.RefreshTokens.Add(newRefreshTokenEntity);
			_dbContext.RefreshTokens.Remove(refreshRecord);
			await _dbContext.SaveChangesAsync();

			Response.Cookies.Append("refreshToken", newRefreshTokenStr, new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.Strict,
				Expires = DateTime.UtcNow.AddDays(7)
			});

			return Ok(new { AccessToken = newAccessToken });
		}
	}
}
