using KBlog.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KBlog.Infrastructure.Persistence
{
	public class ApplicationDbContext : IdentityDbContext<User> {
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { 
		
		}

		public DbSet<Article> Articles { get; set; }
		public DbSet<Category> Categories { get; set; }
		public DbSet<Tag> Tags { get; set; }
		public DbSet<Comment> Comments { get; set; }
		public DbSet<ArticleCategory> ArticleCategories { get; set; }
		public DbSet<ArticleTag> ArticleTags { get; set; }
		public DbSet<FavoriteArticle> FavoriteArticles { get;set; }
		public DbSet<LoginHistory> LoginHistories { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);	

			// Cấu hình khóa chính kết hợp (Composite key) cho các bảng nối
			builder.Entity<ArticleCategory>().HasKey(ac => new { ac.ArticleId, ac.CategoryId });
			builder.Entity<ArticleTag>().HasKey(at => new { at.ArticleId, at.TagId });
			builder.Entity<FavoriteArticle>().HasKey(fa => new { fa.UserId, fa.ArticleId });

			// Cấu hình mối quan hệ many-to-many giữa Article & Category
			builder.Entity<ArticleCategory>().HasOne(ac => ac.Article)
										.WithMany(a => a.ArticleCategories)
										.HasForeignKey(ac => ac.ArticleId);
			builder.Entity<ArticleCategory>().HasOne(ac => ac.Category)
										.WithMany(c => c.ArticleCategories)
										.HasForeignKey(ac => ac.CategoryId);
			
			// Cấu hình quan hệ tự tham chiếu cho Comment (nested comments)
			builder.Entity<Comment>().HasOne(c => c.ParentComment)
									.WithMany(c => c.Replies)
									.HasForeignKey(c => c.ParentCommentId)
									.OnDelete(DeleteBehavior.Restrict); // ngăn việc xóa comment cha nếu có comment con
		}
	}
}
