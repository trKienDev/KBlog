#pragma warning disable CS702

using KBlog.Controllers;
using KBlog.Data;
using KBlog.DTOs;
using KBlog.Models;
using KBlog.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KBlogTest.Controllers
{
	public class UserControllerTest
	{
		// private readonly Mock<IUserService> _userServiceMock;
		private readonly UserController _controller;
		private readonly Mock<IAuthService> _authServiceMock;
		private readonly KBlogDbContext _dbContext;

		public UserControllerTest()
		{
			// Sử dụng InMemoryDatabase để giả lập database
			var options = new DbContextOptionsBuilder<KBlogDbContext>()
							.UseInMemoryDatabase(databaseName: "KBlogTestDb")
							.Options;

			_dbContext = new KBlogDbContext(options);
			_authServiceMock = new Mock<IAuthService>();
			_controller = new UserController(_dbContext, _authServiceMock.Object);
		}

		// Test người dùng đăng ký thành công
		[Fact]
		public async Task Register_Success_ReturnOk()
		{
			// Arrange
			var request = new RegisterRequest
			{
				UserName = "testuser",
				Email = "testuser@example.com",
				Password = "Test@123",
			};
			_authServiceMock.Setup(a => a.GenerateJwtToken(It.IsAny<User>(),
															It.IsAny<string>(),
															It.IsAny<string>()))
															.Returns("mock-jwt-token");
			
			// Act
			var result = await _controller.Register(request);

			// Assert
			var okResult = Assert.IsType<OkObjectResult>(result);
			Assert.Equal(200, okResult.StatusCode);
		}
	}
}
