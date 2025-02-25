namespace KBlog.Services.Interfaces
{
	public interface IWebSocketService
	{
		Task HandleWebSocket(HttpContext context);	
		Task NotifyEmailVerified(string email);
	}
}
