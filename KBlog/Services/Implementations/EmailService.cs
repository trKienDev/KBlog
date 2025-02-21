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

namespace KBlog.Services.Implementations
{
	public class EmailService : IEmailService
	{
		private readonly IConfiguration _configuration;
		private readonly KBlogDbContext _dbContext;
		public EmailService(IConfiguration configuration, KBlogDbContext dbContext)
		{
			_configuration = configuration;
			_dbContext = dbContext;
		}

		public async Task SendEmailAsync(string to, string subject, string body)
		{
			Console.WriteLine($"📌 Email Content: {body}");
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

			int smtpPort = int.TryParse(smtpPortStr, out int port) ? port : 587; // Mặc định 587 nếu lỗi
			bool enableSsl = bool.TryParse(enableSslStr, out bool ssl) ? ssl : true; // Mặc định true nếu lỗi

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
			Console.WriteLine($"🔍 Checking database for token: {token} and email: {email}");

			var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
			if (user == null)
			{
				Console.WriteLine("❌ ERROR: No matching user found.");
				return false;
			}

			if (user.EmailVerificationToken != token)
			{
				Console.WriteLine($"❌ ERROR: Token mismatch! Expected: {user.EmailVerificationToken}, Received: {token}");
				return false;
			}

			Console.WriteLine("✅ User found, updating verification status.");
			user.IsEmailVerified = true;
			user.EmailVerificationToken = null; // Xóa token sau khi xác thực
			await _dbContext.SaveChangesAsync();

			return true;
		}


	}
}
