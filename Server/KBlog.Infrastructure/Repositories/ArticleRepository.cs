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
			return await _context.Articles
				.Include(a => a.Author)
				.Include(a => a.ArticleCategories).ThenInclude(ac => ac.Category)
				.Include(a => a.ArticleTags).ThenInclude(at => at.Tag)
				.OrderByDescending(a => a.CreatedAt)
				.ToListAsync();
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
		public async Task<bool> IsSlugExistAsync(string slug) {
			return await _context.Articles.AnyAsync(a => a.Slug == slug);
		}
	}
}
