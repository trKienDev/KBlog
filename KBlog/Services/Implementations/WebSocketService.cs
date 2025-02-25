using KBlog.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace KBlog.Services.Implementations
{
	public class WebSocketService : IWebSocketService
	{
		private readonly ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();

		public async Task HandleWebSocket(HttpContext context)
		{
			if (context.WebSockets.IsWebSocketRequest)
			{
				using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
				if (!context.Request.Query.ContainsKey("email") || string.IsNullOrWhiteSpace(context.Request.Query["email"]))
				{
					context.Response.StatusCode = 400;
					await context.Response.WriteAsync("Missing or invalid email parameter.");
					return;
				}

				string email = context.Request.Query["email"]!; // Dấu `!` đảm bảo rằng email không null sau kiểm tra
				_sockets[email] = webSocket;

				var buffer = new byte[1024 * 4];
				while (webSocket.State == WebSocketState.Open)
				{
					var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
					if (result.MessageType == WebSocketMessageType.Close)
					{
						_sockets.TryRemove(email, out _);
					}
				}
			}
			else
			{
				context.Response.StatusCode = 400;
			}
		}

		public async Task NotifyEmailVerified(string email) {
			if (_sockets.TryGetValue(email, out var webSocket) && webSocket.State == WebSocketState.Open)
			{
				var message = Encoding.UTF8.GetBytes("verified");
				await webSocket.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true, CancellationToken.None);
			}
		}
	}
}
