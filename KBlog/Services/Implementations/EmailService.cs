using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using KBlog.Services.Interfaces;
using System.Net.Mail;
using System.Net;
using Microsoft.EntityFrameworkCore;
using KBlog.Data;
using KBlog.Models;

namespace KBlog.Services.Implementations
{
	public class EmailService : IEmailService
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IConfiguration _configuration;
		private readonly KBlogDbContext _dbContext;
		public EmailService(IConfiguration configuration, KBlogDbContext dbContext, IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
			_configuration = configuration;
			_dbContext = dbContext;
		}

		public async Task SendEmailAsync(string to, string subject, string body)
		{
			string? smtpServer = _configuration["EmailSettings:SmtpServer"];
			string? smtpPortStr = _configuration["EmailSettings:SmtpPort"];
			string? senderEmail = _configuration["EmailSettings:SenderEmail"];
			string? senderName = _configuration["EmailSettings:SenderName"];
			string? username = _configuration["EmailSettings:Username"];
			string? password = _configuration["EmailSettings:Password"];
			string? enableSslStr = _configuration["EmailSettings:EnableSsl"];

			if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpPortStr) || string.IsNullOrEmpty(senderEmail) ||
			    string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(enableSslStr))
			{
				throw new ArgumentNullException("SMTP configuration is missing in appsettings.json");
			}

			int smtpPort = int.TryParse(smtpPortStr, out int port) ? port : 587; 
			bool enableSsl = bool.TryParse(enableSslStr, out bool ssl) ? ssl : true; 

			var smtpClient = new System.Net.Mail.SmtpClient(smtpServer)
			{
				Port = smtpPort,
				Credentials = new NetworkCredential(username, password),
				EnableSsl = enableSsl
			};

			var mailMessage = new MailMessage
			{
				From = new MailAddress(senderEmail, senderName),
				Subject = subject,
				Body = body,
				IsBodyHtml = true
			};

			mailMessage.To.Add(to);

			await smtpClient.SendMailAsync(mailMessage);
		}

		public async Task<bool> VerifyEmailAsync(string token, string email)
		{
			var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
			if (user == null)
			{
				return false;
			}

			if (user.EmailVerificationToken != token)
			{
				return false;
			}
			user.IsEmailVerified = true;
			user.EmailVerificationToken = null; // Xóa token sau khi xác thực
			await _dbContext.SaveChangesAsync();

			return true;
		}

		public async Task<string> BuildVerificationEmailAsync(string email, string token) {
			var httpContext = _httpContextAccessor.HttpContext;
			if (httpContext == null)
			{
				throw new InvalidOperationException("HttpContext is not available");
			}
			var request = httpContext.Request;
			var verifyLink = $"{request.Scheme}://{request.Host}/api/email/verify?token={token}&email={email}";
			string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Templates", "VerifyEmailTemplate.html");
			string template = await File.ReadAllTextAsync(templatePath);

			return template.Replace("{verifyLink}", verifyLink);
		}
	}
}
