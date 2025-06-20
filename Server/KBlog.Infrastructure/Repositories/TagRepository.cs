using KBlog.Application.Contracts.Persistence;
using KBlog.Domain.Entities;
using KBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KBlog.Infrastructure.Repositories
{
	public class TagRepository : ITagRepository
	{
		private readonly ApplicationDbContext _context;
		public TagRepository(ApplicationDbContext context) { _context = context; }
		public async Task<Tag?> GetByIdAsync(int id) {
			return await _context.Tags.FindAsync(id);
		}
		public async Task<IReadOnlyList<Tag>> GetAllAsync() {
			return await _context.Tags.ToListAsync();
		}
		public async Task<Tag?> FindByNameAsync(string name)
		{
			return await _context.Tags.FirstOrDefaultAsync(t => t.Name == name);
		}
		public async Task<Tag> AddAsync(Tag tag) {
			await _context.AddAsync(tag);
			await _context.SaveChangesAsync();
			return tag;
		}
		public async Task DeleteAsync(Tag tag) {
			_context.Tags.Remove(tag);
			await _context.SaveChangesAsync();
		}
	}
}
