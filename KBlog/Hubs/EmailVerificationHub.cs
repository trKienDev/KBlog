using Microsoft.AspNetCore.SignalR;

namespace KBlog.Hubs
{
	public class EmailVerificationHub : Hub
	{
		// Hàm server gọi để gửi tín hiệu "đã xác thực" về cho client
		// client sẽ lắng nghe method "EmailVerified" (tên tuỳ bạn)
		public async Task SendMessage(string email) {
			// Gửi tới những client nào "quan tâm" (cụ thể: userId = email)
			// => Cần logic gán UserIdentifier hoặc Groups (sẽ hướng dẫn dưới)
			await Clients.User(email).SendAsync("EmailVerified", email);
		}
	}
}
