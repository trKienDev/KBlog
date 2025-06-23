using KBlog.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KBlog.Application.Contracts.Persistence
{
	public interface ILoginHistoryRepository
	{	
		Task AddAsync(LoginHistory loginHistory);	
	}
}
