using System.Collections.Generic;

namespace KBlog.Domain.Entities {
	public class Category
	{
		public int Id { get; set; }
		public string? Name { get; set; }
		public string? Slug { get; set; }
		public string? Description { get; set; }

		// Navigation property for the many-to-many relationshipp
		public ICollection<ArticleCategory> ArticleCategories { get; set; } = new List<ArticleCategory>();
	}
}