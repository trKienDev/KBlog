using KBlog.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KBlog.Application.Contracts.Persistence
{
	public interface ICategoryRepository
	{
		Task<Category?> GetByIdAsync(int id);
		Task<IReadOnlyList<Category>> GetAllAsync();
		Task<Category> AddAsync(Category entity);
		Task UpdateAsync(Category entity);
		Task DeleteAsync(Category entity);
	}
}
