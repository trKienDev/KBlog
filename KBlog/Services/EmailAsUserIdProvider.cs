using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace KBlog.Services
{
	public class EmailAsUserIdProvider : IUserIdProvider
	{
		public string GetUserId(HubConnectionContext connection)
		{
			// Lấy giá trị từ Claims trước
			var emailClaim = connection.User?.FindFirst(ClaimTypes.Email)?.Value;
			if (!string.IsNullOrEmpty(emailClaim))
			{
				return emailClaim;
			}

			// Kiểm tra nếu HttpContext không null trước khi truy cập Request.Query
			var httpContext = connection.GetHttpContext();
			if (httpContext != null && httpContext.Request.Query.ContainsKey("email"))
			{
				return httpContext.Request.Query["email"].ToString();
			}

			// Nếu không có email, trả về giá trị mặc định (tránh null)
			return "unknown_user";
		}
	}
}
