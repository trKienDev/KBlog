using KBlog.Application.DTOs.Auth;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KBlog.Application.Contracts.Identity
{
	public interface IAuthService
	{
		Task<IdentityResult> RegisterAsync(RegisterDto registerDto);
		Task<string?> LoginAsync(LoginDto loginDto);
	}
}
