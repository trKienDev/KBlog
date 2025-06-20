using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KBlog.Application.DTOs.Article
{
	public class CreateArticleDto
	{
		[Required]
		[StringLength(200, MinimumLength = 5)]
		public string Title { get; set; } = string.Empty;

		[Required]
		public string Content {  get; set; } = string.Empty ;

		public string? Excerpt { get; set; } = string.Empty;
		public string? CoverImageUrl { get; set; } = string.Empty;

		public List<int> CategoryIds { get; set; } = new List<int> ();
		public List<string> Tags { get; set; } = new List<string> ();
	}
}
