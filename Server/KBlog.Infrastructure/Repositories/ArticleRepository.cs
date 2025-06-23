using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KBlog.Application.Contracts.Persistence;
using KBlog.Domain.Entities;
using KBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace KBlog.Infrastructure.Repositories
{
	public class ArticleRepository : IArticleRepository
	{
		private readonly ApplicationDbContext _context;
		public ArticleRepository(ApplicationDbContext context) {
			_context = context;
		}

		public async Task<Article> AddAsync(Article entity) {
			await _context.Articles.AddAsync(entity);
			await _context.SaveChangesAsync();
			return entity;
		}
		public async Task DeleteAsync(Article entity) {
			_context.Articles.Remove(entity);
			await _context.SaveChangesAsync();
		}
		public async Task<IReadOnlyList<Article>> GetAllAsync() {
			var base_query = GetBaseArticleQuery();
			return await base_query.ToListAsync();
		}
		public async Task<IReadOnlyList<Article>> GetPagedAsync(int page_number, int page_size) {
			var base_query = GetBaseArticleQuery();
			return await base_query.Skip((page_number - 1) * page_size)
				.Take(page_size).ToListAsync();
		}
		public async Task<Article?> GetByIdAsync(int id) {
			return await _context.Articles
				.Include(a => a.Author)
				.Include(a => a.ArticleCategories)
				.ThenInclude(ac => ac.Category)
				.Include(a => a.ArticleTags)
				.ThenInclude(at => at.Tag)
				.FirstOrDefaultAsync(a => a.Id == id);	
		}
		public async Task<Article?> GetBySlugAsync(string slug) {
			return await _context.Articles
				.Include(a => a.Author)
				.Include (a => a.ArticleCategories)
				.ThenInclude (ac => ac.Category)
				.Include(a => a.ArticleTags)
				.ThenInclude(at => at.Tag)
				.FirstOrDefaultAsync(a => a.Slug == slug);
		}
		public async Task UpdateAsync(Article entity) {
			_context.Entry(entity).State = EntityState.Modified;
			await _context.SaveChangesAsync();
		}
		public async Task<bool> IsSlugExistAsync(string slug, int? articleIdToExclude = null) {

			var query = _context.Articles.Where(a => a.Slug == slug);
			
			// Nếu có ID cần loại trừ --> thêm điều kiện vào câu truy vấn
			if(articleIdToExclude.HasValue) {
				query = query.Where(a => a.Id != articleIdToExclude.Value);	
			}
			return await query.AnyAsync();
		}
		private IQueryable<Article> GetBaseArticleQuery() {
			// AsNoTracking() --> giúp tăng hiệu năng cho các truy vấn chỉ đọc
			return _context.Articles.Include(a => a.Author)
				.Include(a => a.ArticleCategories).ThenInclude(ac => ac.Category)
				.Include(a => a.ArticleTags).ThenInclude(at => at.Tag)
				.OrderByDescending(a => a.CreatedAt);
		}
	}
}
