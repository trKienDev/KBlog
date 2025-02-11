#pragma warning disable CS702

using Azure;
using KBlog.Controllers;
using KBlog.Data;
using KBlog.DTOs;
using KBlog.Models;
using KBlog.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace KBlogTest.Controllers
{
	public class UserControllerTest
	{
		private readonly UserController _controller;
		private readonly Mock<IAuthService> _authServiceMock;
		private readonly KBlogDbContext _dbContext;

		public UserControllerTest()
		{
			// Sử dụng InMemoryDatabase để giả lập database
			var options = new DbContextOptionsBuilder<KBlogDbContext>()
							.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
							.Options;

			_dbContext = new KBlogDbContext(options);
			_authServiceMock = new Mock<IAuthService>();
			_controller = new UserController(_dbContext, _authServiceMock.Object);
		}

		[Fact]
		public async Task Register_Success_ReturnOk()
		{
			// Xoá tất cả dữ liệu trước khi chạy test
			_dbContext.Users.RemoveRange(_dbContext.Users);
			await _dbContext.SaveChangesAsync();

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

		[Fact]
		public async Task Register_EmailExist_ReturnBadRequest()
		{
			// Arrange
			var user = new User { Name = "test", Email = "existing@example.com", Password_hash = "hashedPassword" };

			_dbContext.Users.Add(user);
			await _dbContext.SaveChangesAsync();

			var request = new RegisterRequest
			{
				UserName = "newuser",
				Email = "existing@example.com",
				Password = "Test@123",
			};

			// Act
			var result = await _controller.Register(request);

			// Assert 
			var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal(400, badRequestResult.StatusCode);
		}

		[Fact]
		public async Task Login_Success_ReturnOk()
		{
			// Arrange
			var password = "Test@123";
			var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

			var user = new User
			{
				Name = "testuser",
				Email = "testuser@example.com",
				Password_hash = hashedPassword
			};

			_dbContext.Users.Add(user);
			await _dbContext.SaveChangesAsync();

			var request = new LoginRequest
			{
				Email = "testuser@example.com",
				Password = "Test@123"
			};

			_authServiceMock.Setup(a => a.GenerateJwtToken(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
			    .Returns("mock-jwt-token");

			// Act
			var result = await _controller.Login(request);

			// Assert
			var okResult = Assert.IsType<OkObjectResult>(result);
			var json = JsonConvert.SerializeObject(okResult.Value);
			var response = JsonConvert.DeserializeObject<dynamic>(json);
			string token = response?.token ?? "";
			Assert.Equal("mock-jwt-token", token);
		}

		[Fact]
		public async Task Login_EmailNotExist_ReturnUnauthorized()
		{
			// Arrange
			var request = new LoginRequest
			{
				Email = "nonexistemail@example.com",
				Password = "Test@123"
			};

			// Act
			var result = await _controller.Login(request);

			// Assert
			var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
			Assert.Equal(401, unauthorizedResult.StatusCode);
			Assert.Equal("Invalid email or password", unauthorizedResult.Value);
		}

		[Fact]
		public async Task Login_WrongPassword_ReturnsUnauthorized()
		{
			// Arrange
			var correctPassword = "Test@123";
			var wrongPassword = "Wrong@123";
			var hashedPassword = BCrypt.Net.BCrypt.HashPassword(correctPassword);

			var user = new User
			{
				Name = "testuser",
				Email = "testuser@example.com",
				Password_hash = hashedPassword,
			};
			_dbContext.Users.Add(user);
			await _dbContext.SaveChangesAsync();

			var request = new LoginRequest
			{
				Email = "testuser@example.com",
				Password = wrongPassword,
			};

			// Act
			var result = await _controller.Login(request);

			// Assert
			var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
			Assert.Equal(401, unauthorizedResult.StatusCode);
			Assert.Equal("Invalid email or password", unauthorizedResult.Value);
		}

		[Fact]
		public async Task GetProfile_AuthenticatedUser_ReturnOk()
		{
			// Arrange
			var userId = 1;
			var user = new User
			{
				Id = userId,
				Name = "Test User",
				Email = "testuser@example.com",
			};

			_dbContext.Users.Add(user);
			await _dbContext.SaveChangesAsync();

			var claims = new List<Claim> {
				new Claim(ClaimTypes.NameIdentifier, userId.ToString())
			};
			var identity = new ClaimsIdentity(claims, "TestAuthType");
			var claimsPrincipal = new ClaimsPrincipal(identity);

			_controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext { User = claimsPrincipal }
			};

			// Act
			var result = _controller.GetProfile();

			// Assert
			var okResult = Assert.IsType<OkObjectResult>(result);
			Assert.Equal(200, okResult.StatusCode);

			var json = JsonConvert.SerializeObject(okResult.Value);
			var response = JsonConvert.DeserializeObject<dynamic>(json);

			Assert.NotNull(response);
			int responseId = response?.Id ?? "";
			string responseEmail = response?.Email ?? "";
			string responseName = response?.Name ?? "";
			Assert.Equal(user.Id, responseId);
			Assert.Equal(user.Email, responseEmail);
			Assert.Equal(user.Name, responseName);
		}

		[Fact]
		public void GetProfile_UserIdNOtFoundInToken_ReturnsUnauthorized()
		{
			// Arrange
			var claims = new List<Claim>(); // Ko có Claim NameIdentifier
			var identity = new ClaimsIdentity(claims, "TestAuthType");
			var claimsPrincipal = new ClaimsPrincipal(identity);

			_controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext { User = claimsPrincipal }
			};

			// Act
			var result = _controller.GetProfile();

			// Assert
			var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
			Assert.Equal(401, unauthorizedResult.StatusCode);
			Assert.Equal("User ID not found in token.", unauthorizedResult.Value);
		}

		[Fact]
		public void GetProfile_UserNotFoundInDatabase_ReturnsNotFound()
		{
			// Arrange
			var userId = 0;
			var claims = new List<Claim> {
				new Claim(ClaimTypes.NameIdentifier, userId.ToString())
			};
			var identity = new ClaimsIdentity(claims, "TestAuthType");
			var claimsPrincipal = new ClaimsPrincipal(identity);

			_controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext { User = claimsPrincipal }
			};

			// Act
			var result = _controller.GetProfile();

			// Assert
			var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
			Assert.Equal(404, notFoundResult.StatusCode);
			var json = JsonConvert.SerializeObject(notFoundResult.Value);
			var message = JsonConvert.DeserializeObject<dynamic>(json);
			Assert.Equal("User not found.", message);
		}

		[Fact]
		public async Task UpdateUser_Success_ReturnOk()
		{
			_dbContext.Users.RemoveRange(_dbContext.Users);
			await _dbContext.SaveChangesAsync();
			// Arrange
			var user = new User
			{
				Id = 1,
				Name = "Old Name",
				Email = "oldemail@example.com",
			};
			_dbContext.Users.Add(user);
			await _dbContext.SaveChangesAsync();

			var updateUserDTO = new UpdateUser
			{
				UserName = "New Name",
				Email = "newemail@example.com",
			};

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
			};
			var identity = new ClaimsIdentity(claims, "TestAuthType");
			var claimsPrincipal = new ClaimsPrincipal(identity);
			_controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext { User = claimsPrincipal }
			};

			// Act 
			var result = _controller.UpdateUser(user.Id, updateUserDTO);

			// Assert
			var okResult = Assert.IsType<OkObjectResult>(result);
			Assert.Equal(200, okResult.StatusCode);
			var json = JsonConvert.SerializeObject(okResult);
			var response = JsonConvert.DeserializeObject<dynamic>(json);
			Assert.NotNull(response);
			var responseValue = response?.Value ?? "";
			string responseEmail = responseValue?.Email ?? "";
			string responseName = responseValue?.Name ?? "";
			Assert.Equal(user.Email, responseEmail);
			Assert.Equal(user.Name, responseName);
		}

		[Fact]
		public void UpdateUser_UserNotAuthenticated_ReturnsUnauthorized()
		{
			// Arrange
			var updateUserDTO = new UpdateUser
			{
				UserName = "New Name",
				Email = "newemail@example.com",
			};

			_controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext() // ko có ClaimsPrinccipal
			};

			// Act
			var result = _controller.UpdateUser(1, updateUserDTO);

			// Assert
			var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
			Assert.Equal(401, unauthorizedResult.StatusCode);
			Assert.Equal("User ID not found in token.", unauthorizedResult.Value);
		}

		[Fact]
		public async Task UpdateUser_UnauthorizedUser_ReturnForbid()
		{
			// Arrange
			var user = new User
			{
				Id = 1,
				Name = "Old Name",
				Email = "oldemail@example.com",
			};
			_dbContext.Users.Add(user);
			await _dbContext.SaveChangesAsync();

			var updateUserDTO = new UpdateUser
			{
				UserName = "New Name",
				Email = "newemail@example.com",
			};

			var claims = new List<Claim> {
				new Claim(ClaimTypes.NameIdentifier, "2") // User khác
			};
			var identity = new ClaimsIdentity(claims, "TetsAuthType");
			var claimsPrinciple = new ClaimsPrincipal(identity);
			_controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext { User = claimsPrinciple }
			};

			// Act
			var result = _controller.UpdateUser(user.Id, updateUserDTO);

			// Assert
			var forbidResult = Assert.IsType<ForbidResult>(result);
		}

		[Fact]
		public async Task UpdateUser_EmptyUpdate_ReturnsBadRequest()
		{
			// Arrange
			var user = new User
			{
				Id = 1,
				Name = "Old Name",
				Email = "oldemail@example.com",
			};
			_dbContext.Users.Add(user);
			await _dbContext.SaveChangesAsync();

			var claims = new List<Claim> {
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
			};
			var identity = new ClaimsIdentity(claims, "TestAuthType");
			var claimsPrincipal = new ClaimsPrincipal(identity);
			_controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext() { User = claimsPrincipal }
			};

			// Act
			var result = _controller.UpdateUser(user.Id, null); // Null DTO

			// Assert
			var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal(400, badRequestResult.StatusCode);
			Assert.Equal("Name or Email must be provided.", badRequestResult.Value);
		}

		[Fact]
		public async Task DeleteUser_Admin_Success_ReturnNoContent()
		{
			// Arrange
			var adminUserId = "1";
			var userToDelete = new User
			{
				Id = 2,
				Name = "Test User",
				Email = "testuser@example.com",
			};

			_dbContext.Users.Add(userToDelete);
			await _dbContext.SaveChangesAsync();

			var claims = new List<Claim> {
				new Claim(ClaimTypes.NameIdentifier, adminUserId),
				new Claim(ClaimTypes.Role, "Admin")
			};
			var identity = new ClaimsIdentity(claims, "TestAuthType");
			var claimsPrincipal = new ClaimsPrincipal(identity);
			_controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext { User = claimsPrincipal }
			};

			// Act
			var result = _controller.DeleteUser(userToDelete.Id);

			// Assert
			Assert.IsType<NoContentResult>(result);
		}

		[Fact]
		public async Task DeleteUser_Owner_Success_ReturnNoContent()
		{
			// Arrange
			var userId = "1";
			var user = new User
			{
				Id = int.Parse(userId),
				Name = "Test User",
				Email = "testuser@example.com",
			};
			_dbContext.Users.Add(user);
			await _dbContext.SaveChangesAsync();

			var claims = new List<Claim> {
				new Claim(ClaimTypes.NameIdentifier, userId),
			};
			var identity = new ClaimsIdentity(claims, "TestAuthType");
			var claimsPrincipal = new ClaimsPrincipal(identity);
			_controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext { User = claimsPrincipal }
			};

			// Act
			var result = _controller.DeleteUser(user.Id);

			// Assert
			Assert.IsType<NoContentResult>(result);
		}

		[Fact]
		public void DeleteUser_UserNotAuthenticated_ReturnUnauthorized()
		{
			// Arrange
			_controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext() // Ko cos user token
			};

			// Act
			var result = _controller.DeleteUser(1);

			// Assert
			var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
			Assert.Equal(401, unauthorizedResult.StatusCode);
			Assert.Equal("User ID not found in token.", unauthorizedResult.Value);
		}

		[Fact]
		public void DeleteUser_UserNotFoun_ReturnsNotFound()
		{
			// Arrange
			var UserId = "1";
			var claims = new List<Claim> {
				new Claim(ClaimTypes.NameIdentifier, UserId),
				new Claim(ClaimTypes.Role, "Admin"),
			};
			var identity = new ClaimsIdentity(claims, "TestAuthType");
			var claimsPrincipal = new ClaimsPrincipal(identity);
			_controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext { User = claimsPrincipal }
			};

			// Act
			var result = _controller.DeleteUser(999); // Id không tồn tại

			// Assert
			var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
			Assert.Equal(404, notFoundResult.StatusCode);
			Assert.Equal("User not found.", notFoundResult?.Value);
		}

		[Fact]
		public async Task DeleteUser_UnauthorizedUser_ReturnsForbid()
		{
			// Arrange
			var ownerUserId = "1";
			var otherUserId = "2";
			var user = new User
			{
				Id = int.Parse(ownerUserId),
				Name = "Test User",
				Email = "testuser@example.com",
			};

			_dbContext.Users.Add(user);
			await _dbContext.SaveChangesAsync();

			var claims = new List<Claim> {
				new Claim(ClaimTypes.NameIdentifier, otherUserId) // ko phải Admin & ko phải chủ tài khoản	
			};
			var identity = new ClaimsIdentity(claims, "TestAuthType");
			var claimsPrincipal = new ClaimsPrincipal(identity);
			_controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext { User = claimsPrincipal }
			};

			// Act
			var result = _controller.DeleteUser(user.Id);

			// Assert
			var forbidResult = Assert.IsType<ForbidResult>(result);
		}

		// Xoá dữ liệu sau mỗi lần test để đảm bảo tính độc lập
		[Fact]
		public void Dispose()
		{
			_dbContext.Database.EnsureDeleted(); // Xóa database sau mỗi lần test
			_dbContext.Dispose();
		}
	}
}
