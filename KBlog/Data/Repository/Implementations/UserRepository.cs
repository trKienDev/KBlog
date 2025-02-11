using KBlog.Data.Repository.Interfaces;
using KBlog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace KBlog.Data.Repository.Implementations
{
	public class UserRepository : IUserRepository
	{
		private readonly KBlogDbContext _dbContext;
		public UserRepository(KBlogDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<User?> GetUserByEmailAsync(string email)
		{
			return await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
		}

		public async Task<User?> GetUserByIdAsync(int id)
		{
			return await _dbContext.Users.FindAsync(id);
		}

		public async Task<IEnumerable<User>> GetAllUsersAsync() {
			return await _dbContext.Users.ToListAsync();
		}

		public async Task AddUserAsync(User user)
		{
			await _dbContext.Users.AddAsync(user);
		}

		public async Task SaveChangesAsync()
		{
			await _dbContext.SaveChangesAsync();
		}

		public async Task DeleteUserAsync(int id)
		{
			var user = await _dbContext.Users.FindAsync(id);
			if (user != null)
			{
				_dbContext.Users.Remove(user);
			}
		}
	}
}
