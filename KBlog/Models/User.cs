namespace KBlog.Models
{
	public class User
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Password_hash { get; set; } = string.Empty;
		public string? Avatar_url { get; set; } // Nullable
		public string Role { get; set; } = "user"; // default: user
		public DateTime CreatedAt { get; set; } = DateTime.Now;
	}
}
