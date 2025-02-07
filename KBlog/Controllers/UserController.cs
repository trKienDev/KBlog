using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Net.WebSockets;
using KBlog.Models;
using KBlog.DTOs;
using KBlog.Data;
using Microsoft.EntityFrameworkCore;
using KBlog.Services;
using System.Runtime.CompilerServices;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace KBlog.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly KBlogDbContext _dbContext;
		private readonly IAuthService _authService;

		public UserController(KBlogDbContext context, IAuthService authService) {
			_dbContext = context;
			_authService = authService;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterRequest model) {
			var existingUser = await _dbContext.Users.FirstOrDefaultAsync(user => user.Email == model.Email);

			if (existingUser != null)
				return BadRequest("Email exists !");

			var user = new User
			{
				Name = model.UserName,
				Email = model.Email,
				Password_hash = BCrypt.Net.BCrypt.HashPassword(model.Password),
			};

			_dbContext.Users.Add(user);
			await _dbContext.SaveChangesAsync();

			return Ok(new { message = "Register Successfully!" });
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequest model) {
			var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
			if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password_hash))
				return Unauthorized("Invalid email or password");

			var token = _authService.GenerateJwtToken(user, user.Email, user.Name);

			return Ok(new { token });
		}

		[Authorize]
		[HttpGet("profile")]
		public IActionResult GetProfile()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized("User ID not found in token.");
			}

			var user = _dbContext.Users.FirstOrDefault(u => u.Id.ToString() == userId);
			if (user == null)
			{
				Console.WriteLine("User not found in database.");
				return NotFound("User not found.");
			}

			return Ok(new
			{
				user.Id,
				user.Email,
				user.Name
			});
		}

		[Authorize]
		[HttpGet("all")]
		public async Task<IActionResult> GetAllUsers() {
			var users = await _dbContext.Users.Select(user => new
			{
				id = user.Id,
				userName = user.Name,
				email = user.Email
			}).ToListAsync();

			return Ok(users);
		}

		[Authorize]
		[HttpPut("{id}")]
		public IActionResult UpdateUser(int id, [FromBody] UpdateUser? updateUserDTO) {
			if(updateUserDTO == null) {
				return BadRequest("Name or Email must be provided.");	
			}
			var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(currentUserId))
			{
				return Unauthorized("User ID not found in token.");
			}

			var user = _dbContext.Users.FirstOrDefault(u => u.Id == id);
			if (user == null)	
			{
				return NotFound("User not found");
			}

			if (currentUserId != user.Id.ToString())
			{
				return Forbid("You are not allowed to update this user.");
			}
			if(updateUserDTO == null){
				return BadRequest("Name or Email must be provided");
			}

			if (!string.IsNullOrEmpty(updateUserDTO.UserName)) {
				user.Name = updateUserDTO.UserName;
			}
			if (!string.IsNullOrEmpty(updateUserDTO.Email)) {
				user.Email = updateUserDTO.Email;
			}
			_dbContext.SaveChanges();

			return Ok(new {
				user.Id,
				user.Name,
				user.Email,
				user.CreatedAt,
			});
		}

		[Authorize]
		[HttpDelete("{id}")]
		public IActionResult DeleteUser(int id) {
			var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(currentUserId))
			{
				return Unauthorized("User ID not found in token.");
			}

			var user = _dbContext.Users.FirstOrDefault(u => u.Id ==id);
			if (user == null)
			{
				return NotFound("User not found.");
			}

			var isAdmin = User.IsInRole("Admin");
			Console.WriteLine($"Admin: {isAdmin}");
			Console.WriteLine($"currentUserId: {currentUserId}");
			Console.WriteLine($"userId: {user.Id.ToString()}");
			if (!isAdmin && currentUserId != user.Id.ToString()) // Nếu ko phải admin hoặc chính chủ thì ko được xoá tài khoản
			{
				return Forbid("You are not allowed to delete this user.");
			}

			_dbContext.Users.Remove(user);
			_dbContext.SaveChanges();

			return NoContent();
		}
	}
}
