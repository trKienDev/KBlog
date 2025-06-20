using System;

namespace KBlog.Domain.Entities {
	public class Article
	{
		public int Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Slug { get; set; } = string.Empty;
		public string Content { get; set; } = string.Empty;
		public string? Excerpt { get; set; } = string.Empty;
		public string? CoverImageUrl { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public DateTime? PublishedAt { get; set; }
		public PostStatus Status { get; set; }

		// Foreign Key of User (author)
		public string? AuthorId { get; set; }

		// Navigation property
		public User? Author { get; set; }
		public ICollection<ArticleCategory> ArticleCategories { get; set; } = new List<ArticleCategory>();
		public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
		public ICollection<Comment> Comments { get; set; } = new List<Comment>();
		public ICollection<FavoriteArticle> FavoriteArticles { get; set; } = new List<FavoriteArticle>();
	}

	public enum PostStatus
	{
		Draft, 
		Published,
		Scheduled, 
		Archived, 
	}
}