using KBlog.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KBlog.Application.Contracts.Persistence
{
	public interface ITagRepository
	{
		Task<Tag?> GetByIdAsync(int id);
		Task<IReadOnlyList<Tag>> GetAllAsync();
		Task<Tag?> FindByNameAsync(string name);
		Task<Tag> AddAsync(Tag tag);
		Task DeleteAsync(Tag tag);
	}
}
