using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KBlog.Domain.Entities;

namespace KBlog.Application.Contracts.Persistence
{
	public interface IArticleRepository
	{
		Task<Article?> GetByIdAsync(int id);
		Task<Article?> GetBySlugAsync(string slug);
		Task<IReadOnlyList<Article>> GetAllAsync();
		Task<Article> AddAsync(Article entity);
		Task UpdateAsync(Article entity);
		Task DeleteAsync(Article entity);
		Task<bool> IsSlugExistAsync(string slug);	
	}
}
