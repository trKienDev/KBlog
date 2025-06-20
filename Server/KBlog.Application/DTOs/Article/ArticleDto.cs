using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KBlog.Application.DTOs.Article
{
	public class ArticleDto
	{
		public int Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Slug { get; set; } = string.Empty;
		public string Content { get; set; } = string.Empty;
		public string? Excerpt {  get; set; }
		public string? CoverImageUrl {  get; set; }
		public DateTime CreatedAt { get; set; }
		public string AuthorName { get; set; } = string.Empty;
		public List<string>	Categories { get; set; } = new List<string>();
		public List<string> Tags { get; set; } = new List<string>();
	}
}
