using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KBlog.Domain.Entities
{
	public class LoginHistory
	{
		public long Id { get; set; } // Lưu được nhiều phiên lịch sử
		public DateTime LoginTime { get; set; }
		public string? IpAddress	 {  get; set; }
		
		// Foreign key đến Users
		public string UserId { get; set; } = string.Empty;
		public User? User { get; set; }
	}
}
