using KBlog.Data;
using KBlog.Hubs;
using KBlog.Services.Implementations;
using KBlog.Services.Interfaces;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace KBlog.Controllers
{
	[Route("api/email")]
	[ApiController]
	public class EmailController : Controller
	{
		private readonly IHubContext<EmailVerificationHub> _hubContext;
		private readonly IEmailService _emailService;

		public EmailController(IHubContext<EmailVerificationHub> hubContext, IEmailService emailService)
		{
			_hubContext = hubContext;
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
			bool isVerified = await _emailService.VerifyEmailAsync(token, email);
			if (!isVerified)
			{
				return Content(@"
				<html>
					<head>
						<meta http-equiv='refresh' content='3;url=https://yourdomain.com/email-verification-failed' />
						<style>
							body { text-align: center; padding: 50px; font-family: Arial, sans-serif; }
							.message { color: red; font-size: 20px; }
						</style>
					</head>
					<body>
						<h2 class='message'>❌ Xác thực thất bại! Link không hợp lệ hoặc đã hết hạn.</h2>
						<p>Bạn sẽ được chuyển về trang đăng ký trong 3 giây...</p>
					</body>
				</html>", "text/html");
			}

			// Gửi tín hiệu cho Tab A (trang Register) - dựa trên email
			//    Tương ứng logic Clients.User(email).SendAsync("EmailVerified", email)
			await _hubContext.Clients.User(email).SendAsync("EmailVerified", email);

			// 2) Xử lý kết quả cho Tab B
			//    Ở đây, tuỳ bạn muốn show HTML "Success" hay redirect
			//    redirect -> Mở "http://localhost:5173/email-verified?email=..." (tab B)
			//    hoặc hiển thị HTML
			// Ví dụ ta trả về 1 trang HTML đơn giản
			return Redirect("http://localhost:5173/email-verified");
		}

		public class EmailRequest
		{
			public string To { get; set; } = string.Empty;
			public string Subject { get; set; } = string.Empty;
			public string Body { get; set; } = string.Empty;
		}
	}
}
