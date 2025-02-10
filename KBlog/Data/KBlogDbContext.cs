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

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			// Cấu hình bảng hoặc ràng buộc nếu cần
		}
	}
}
