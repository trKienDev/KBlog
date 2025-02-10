using System.Data;

namespace KBlog.DTOs
{
	public class CreatePostDTO
	{
		public string Title { get; set; } = string.Empty;
		public string Content { get; set; } = string.Empty;
		public int userId { get; set; }
	}

	public class UpdatePostDTO
	{
		public string Title { get; set; } = string.Empty;
		public string Content { get; set; } = string.Empty;
	}

	// Trả dữ liệu về api
	public class PostDTO
	{
		public int Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Content { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		public int UserId { get; set; }
		public string UserName { get; set; } = string.Empty;
	}


}
