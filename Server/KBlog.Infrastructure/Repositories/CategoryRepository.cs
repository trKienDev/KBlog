using KBlog.Application.Contracts.Persistence;
using KBlog.Domain.Entities;
using KBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KBlog.Infrastructure.Repositories
{
	public class CategoryRepository : ICategoryRepository
	{
		private readonly ApplicationDbContext _context;
		public CategoryRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<Category> AddAsync(Category entity) {
			await _context.Categories.AddAsync(entity);
			await _context.SaveChangesAsync();
			return entity;
		}
		public async Task DeleteAsync(Category entity) {
			_context.Categories.Remove(entity);	
			await _context.SaveChangesAsync();	
		}
		public async Task<IReadOnlyList<Category>> GetAllAsync() {
			return await _context.Categories.ToListAsync();
		}
		public async Task<Category?> GetByIdAsync(int id) {
			return await _context.Categories.FindAsync(id);
		}
		public async Task UpdateAsync(Category entity) {
			// Đánh dấu đối tượng là đã bị thay đổi
			_context.Entry(entity).State = EntityState.Modified;
			await _context.SaveChangesAsync();
		}
	}
}
