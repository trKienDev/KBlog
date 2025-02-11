using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KBlog.Models
{
	public class Post
	{
		[Key]
		public int Id { get; set; }

		[Required]
		[MaxLength(200)]
		public string Title { get; set; } = string.Empty;

		[Required]
		public string Content { get; set; } = string.Empty;

		[Required]
		public string Slug { get; set; } = string.Empty;

		[ForeignKey("User")]
		public int UserId { get; set; }	

		[ForeignKey("Category")]
		public int CategoryId { get; set; }
		//public virtual Category Category{ get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
	}
}
