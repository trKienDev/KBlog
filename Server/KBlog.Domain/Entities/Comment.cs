using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KBlog.Domain.Entities
{
	public class Comment
	{
		public int Id { get; set; }
		public string Content { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		
		// Foreign Key to Article
		public int ArticleId { get; set; }
		public Article? Article { get; set; }

		// Foreign key to Users (who write comments)
		public int? ParentCommentId { get; set; } // Null nếu comment là gốc
		public Comment? ParentComment { get; set; }

		// Navigation property cho các comment trả lời
		public ICollection<Comment>? Replies { get; set; }
	}
}
