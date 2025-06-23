using KBlog.Application.Contracts.Persistence;
using KBlog.Domain.Entities;
using KBlog.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KBlog.Infrastructure.Repositories
{
	public class LoginHistoryRepository : ILoginHistoryRepository
	{
		private readonly ApplicationDbContext _context;
		public LoginHistoryRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(LoginHistory loginHistory) {
			await _context.LoginHistories.AddAsync(loginHistory);
			await _context.SaveChangesAsync();
		}
	}
}
