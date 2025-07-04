﻿using System;
using KBlog.Domain.Entities;

namespace KBlog.Domain.Entities
{
	public class ArticleCategory
	{
		public int ArticleId { get; set; }
		public Article? Article { get; set; }
		public int CategoryId { get; set; }
		public Category? Category { get; set; }
	}
}