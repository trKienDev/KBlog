using Microsoft.EntityFrameworkCore;
using KBlog.Models;

namespace KBlog.Data
{
	public class KBlogDbContext : DbContext
	{
		public KBlogDbContext(DbContextOptions<KBlogDbContext> options)
		    : base(options)
		{
		}

		// Định nghĩa các bảng trong database (DbSet<T>)
		public DbSet<User> Users { get; set; }
		public DbSet<Post> Posts { get; set; }
		public DbSet<Comment> Comments { get; set; }	

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Comment>().HasOne(c => c.Post)
										.WithMany(p =>p.Comments)
										.HasForeignKey(c => c.PostId)
										.OnDelete(DeleteBehavior.Cascade); // xoá post thì xoá tất cả comment của nó
			
			modelBuilder.Entity<Comment>().HasOne(c => c.User)
										.WithMany()
										.HasForeignKey(c => c.UserId)
										.OnDelete(DeleteBehavior.Restrict); // Ko cho phép xoá user nếu có comment

			modelBuilder.Entity<Comment>().HasOne(c => c.ParrentComment)
										.WithMany(c => c.Replies)
										.HasForeignKey(c => c.ParentId)
										.OnDelete(DeleteBehavior.NoAction); // Ko tự động xoá comment cha							
		}
	}
}
