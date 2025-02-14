using System.ComponentModel.DataAnnotations;

namespace KBlog.DTOs
{
	public class CommentDTO
	{
		public int Id { get; set; }
		public int PostId { get; set; }
		public int UserId { get; set; }
		public string Content { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public int? ParentId { get; set; }
	}

	public class CreateCommentDTO {
		[Required]
		public string Content { get; set; } = string.Empty;
	}

	public class  UpdateCommentDTO
	{
		[Required]
		[MaxLength(10000)]
		public string Content { get; set; } = string.Empty;
	}
}
