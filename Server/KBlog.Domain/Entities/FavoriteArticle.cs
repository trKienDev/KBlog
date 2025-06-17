using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KBlog.Domain.Entities
{
	public class FavoriteArticle
	{
		// Foreign Keys tạo thành Composite Primary Key
		public string? UserId { get; set; }
		public int ArticleId { get; set; }

		// Navigation properties
		public User? User { get; set; }
		public Article? Article { get; set; }

	}
}
