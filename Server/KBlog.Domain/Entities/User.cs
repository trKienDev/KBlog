using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace KBlog.Domain.Entities
{
	public class User : IdentityUser
	{
		public string Fulname { get; set; } = string.Empty;
		public DateTime RegisteredAt { get; set; }

		public ICollection<Article>? Articles { get; set; }
		public ICollection<Comment>? Comments { get; set; }
		public ICollection<FavoriteArticle>? FavoriteArticles { get; set; }
		public ICollection<LoginHistory>? LoginHistories { get; set; }
	}
}
