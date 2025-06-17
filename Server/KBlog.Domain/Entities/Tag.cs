using System.Collections.Generic;

namespace KBlog.Domain.Entities
{
	public class Tag
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Slug { get; set; } = string.Empty;

		// Navigation property for the many-to-many relationship
		public ICollection<ArticleTag>? ArticleTags { get; set; }
	}
}