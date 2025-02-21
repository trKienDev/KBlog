namespace KBlog.Services.Interfaces
{
	public interface IEmailService
	{
		Task SendEmailAsync(string toEmail, string subject, string message);
		Task<bool> VerifyEmailAsync(string token, string email);
	}
}
