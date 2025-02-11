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

namespace KBlog.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly KBlogDbContext _dbContext;
		private readonly IAuthService _authService;
		private readonly IUserService _userService;

		public UserController(KBlogDbContext context, IAuthService authService, IUserService userService) {
			_dbContext = context;
			_authService = authService;
			_userService = userService;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterRequest model) {
			try
			{
				var existingUser = await _userService.GetUserByEmailAsync(model.Email);
				if (existingUser != null)
				{
					return BadRequest("Email exists!");
				}
				await _userService.RegisterUserAsync(model);
				return Ok(new { message = "Register Successfully" });
			}
			catch (System.Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequest model) {
			if (model.Email == null) {
				return BadRequest("Please provide email!");
			} else if (model.Password == null) {
				return BadRequest("Please provide password");
			}

			var user = await _userService.GetUserByEmailAsync(model.Email);
			if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password_hash))
			{
				return Unauthorized("Invalid email or password");
			}

			var token = _authService.GenerateJwtToken(new User { Id = user.Id, Email = user.Email, Name = user.Name }, user.Email, user.Name);
			return Ok(new { token });
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
		public async Task<IActionResult> GetAllUsers() {
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
	}
}
