using KBlog.Data;
using KBlog.Services.Implementations;
using KBlog.Services.Interfaces;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KBlog.Controllers
{
	[Route("api/email")]
	[ApiController]
	public class EmailController : Controller
	{
		private readonly IEmailService _emailService;

		public EmailController(IEmailService emailService)
		{
			_emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
		}

		[HttpPost("send")]
		public async Task<IActionResult> SendEmail([FromBody] EmailRequest request) {
			await _emailService.SendEmailAsync(request.To, request.Subject, request.Body);
			return Ok( new { success= true, message="Email sent successfully" });
		}
		
		[HttpGet("verify")]
		public async Task<IActionResult> VerifyEmail(string token, string email)
		{
			Console.WriteLine($"📩 Received Verification Request");
			Console.WriteLine($"📌 Token from URL: {token}");
			Console.WriteLine($"📌 Email from URL: {email}");

			bool isVerified = await _emailService.VerifyEmailAsync(token, email);
			if (!isVerified)
			{
				Console.WriteLine("❌ ERROR: Invalid token or expired.");
				return BadRequest(new { message = "Invalid verification link or token expired." });
			}

			Console.WriteLine("✅ Email verified successfully.");
			return Ok(new { message = "Email verified successfully. You can now log in." });
		}

		public class EmailRequest
		{
			public string To { get; set; } = string.Empty;
			public string Subject { get; set; } = string.Empty;
			public string Body { get; set; } = string.Empty;
		}
	}
}
