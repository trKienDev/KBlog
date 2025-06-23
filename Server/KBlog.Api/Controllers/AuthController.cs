using KBlog.Application.Contracts.Identity;
using KBlog.Application.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;

namespace KBlog.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController(IAuthService authService) : ControllerBase
	{
		private readonly IAuthService _authService = authService;

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterDto registerDto) {
			var result = await _authService.RegisterAsync(registerDto);
			if (result.Succeeded)
			{
				return StatusCode(201, new { Message = "User registered successfully" });
			}
			var errorMessages = result.Errors.Select(e => e.Description).ToList();
			return BadRequest(new { Errors = errorMessages });	
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginDto loginDto) {
			var token = await _authService.LoginAsync(loginDto);
			if(token == null) {
				return Unauthorized("Invalid credentials");
			} 
			return Ok(new { token });
		}
	}
}
