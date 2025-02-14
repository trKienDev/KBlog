using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KBlog.Models
{
	public class Comment
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public int PostId { get; set; }

		[Required]
		public int? UserId { get; set; } // Cho phép null khi user bị xoá

		public int? ParentId { get; set; }

		[Required]
		[MaxLength(10000)]
		public string Content { get; set; } = string.Empty;
		public DateTime CreateAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }	

		// Navigation properties
		[ForeignKey("PostId")]
		public virtual Post? Post { get; set; }

		[ForeignKey("UserId")]
		public virtual User? User { get; set; }

		[ForeignKey("ParentId")]
		public virtual Comment? ParrentComment { get; set; }	

		public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
	}
}
