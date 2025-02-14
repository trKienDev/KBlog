using KBlog.Controllers;
using KBlog.Data;
using KBlog.DTOs;
using KBlog.Models;
using KBlog.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Moq;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace KBlogTest.Controllers {
	public class UserControllerTests
	{
		private readonly Mock<IUserService> _mockUserService;
		private readonly Mock<IAuthService> _mockAuthService;
		private readonly UserController _controller;
		private readonly KBlogDbContext _dbContext;

		public UserControllerTests()
		{
			var options = new DbContextOptionsBuilder<KBlogDbContext>()
						.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
						.Options;
			_dbContext = new KBlogDbContext(options);
			_mockUserService = new Mock<IUserService>();
			_mockAuthService = new Mock<IAuthService>();
			_controller = new UserController(_dbContext, _mockAuthService.Object, _mockUserService.Object);
		}

		// Mock User loging in
		private void SetUserContext(int userId, bool isAdmin = false)
		{
			var claims = new List<Claim> {
				new Claim(ClaimTypes.NameIdentifier, userId.ToString())
			};

			if(isAdmin) {
				claims.Add(new Claim(ClaimTypes.Role, "Admin"));
			}

			var identity = new ClaimsIdentity(claims, "TestAUth");
			var userPrincipal = new ClaimsPrincipal(identity);

			var httpContext = new DefaultHttpContext
			{
				User = userPrincipal
			};
			_controller.ControllerContext = new ControllerContext
			{
				HttpContext = httpContext
			};
		}

		[Fact]
		public async Task Register_Success_ReturnsOkResult()
		{
			// Arrange
			var request = new RegisterRequest
			{
				Email = "test@example.com",
				Password = "Password123",
				UserName = "Test user",
			};

			_mockUserService.Setup(x => x.GetUserByEmailAsync(request.Email))
							.ReturnsAsync((UserDTO?)null); // Explicity nullable
			_mockUserService.Setup(x => x.RegisterUserAsync(request))
							.Returns(Task.CompletedTask);

			// Act
			var result = await _controller.Register(request);

			// Assert
			var okResult = Assert.IsType<OkObjectResult>(result);
			Assert.NotNull(okResult.Value);
			var json = JsonConvert.SerializeObject(okResult.Value);
			var response = JsonConvert.DeserializeObject<dynamic>(json);
			if (response != null)
			{
				string message = response.message;
				Assert.Equal("Register Successfully", message);
			}
			else
			{
				Assert.Fail("Unexpected response type");
			}
		}

		[Fact]
		public async Task Register_EmailExists_ReturnsBadRequest()
		{
			// Arrange
			var request = new RegisterRequest
			{
				Email = "test@example.com",
				Password = "Password123",
				UserName = "Test User",
			};
			_mockUserService.Setup(x => x.GetUserByEmailAsync(request.Email))
							.ReturnsAsync(new UserDTO { Email = request.Email });

			// Act
			var result = await _controller.Register(request);

			// Assert
			var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
			Assert.NotNull(badRequestResult.Value);
			Assert.Equal("Email exists!", badRequestResult.Value);
		}

		[Fact]
		public async Task Register_ExceptionThrown_ReturnsBadRequest()
		{
			// Arrange
			var request = new RegisterRequest
			{
				Email = "test@example.com",
				Password = "Password123",
				UserName = "Test User",
			};
			_mockUserService.Setup(x => x.GetUserByEmailAsync(request.Email))
							.ThrowsAsync(new System.Exception("Unexpected error"));

			// Act
			var result = await _controller.Register(request);

			// Assert
			var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
			Assert.NotNull(badRequestResult.Value);
			Assert.Equal("Unexpected error", badRequestResult.Value);
		}

		[Fact]
		public async Task Login_MissingEmail_ReturnsBadRequest()
		{
			// Arrange
			var request = new LoginRequest
			{
				Email = null,
				Password = "Password123",
			};

			// Act
			var result = await _controller.Login(request);

			// Assert
			var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
			Assert.NotNull(badRequestResult.Value);
			Assert.Equal("Please provide email!", badRequestResult.Value);
		}

		[Fact]
		public async Task Login_MissingPassword_ReturnsBadRequest()
		{
			// Arrange
			var request = new LoginRequest
			{
				Email = "test@example.com",
				Password = null,
			};

			// Act
			var result = await _controller.Login(request);

			// Assert
			var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
			Assert.NotNull(badRequestResult.Value);
			Assert.Equal("Please provide password", badRequestResult.Value);
		}

		[Fact]
		public async Task Login_InvalidCredentials_ReturnsUnauthorized()
		{
			// Arrange
			var request = new LoginRequest
			{
				Email = "test@example.com",
				Password = "WrongPassword",
			};
			_mockUserService.Setup(x => x.GetUserByEmailAsync(request.Email))
							.ReturnsAsync(new UserDTO
							{
								Email = request.Email,
								Password_hash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword")
							});
			// Act
			var result = await _controller.Login(request);

			// Assert
			var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
			Assert.NotNull(unauthorizedResult.Value);
			Assert.Equal("Invalid email or password", unauthorizedResult.Value);
		}

		[Fact]
		public async Task Login_ValidCredentials_ReturnsToken()
		{
			// Arrange
			var request = new LoginRequest
			{
				Email = "test@example.com",
				Password = "CorrectPassword",
			};

			var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
			var userDTO = new UserDTO
			{
				Id = 1,
				Email = request.Email,
				Name = "Test user",
				Password_hash = hashedPassword
			};

			_mockUserService.Setup(x => x.GetUserByEmailAsync(request.Email))
							.ReturnsAsync(userDTO);
			_mockAuthService.Setup(x => x.GenerateJwtToken(It.IsAny<User>(), userDTO.Email, userDTO.Name))
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
		public async Task GetProfile_UserNotAuthenticated_ReturnsUnauthorized()
		{
			// Arrage: ko thiết lập User Context --> giả lập ko có token
			_controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext()
			};

			// Act
			var result = await _controller.GetProfile();

			// Assert
			var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
			Assert.NotNull(unauthorizedResult);
			Assert.Equal("User ID not found in token.", unauthorizedResult.Value);
		}

		[Fact]
		public async Task GetProfile_UserNotFound_ReturnsNotFound()
		{
			// Arrange: thiết lập User Context với 1 User ID hợp lệ
			SetUserContext(1);

			_mockUserService.Setup(x => x.GetUserByIdAsync(1))
							.ReturnsAsync((UserDTO?)null); // Giả lập user ko tồn tại

			// Act
			var result = await _controller.GetProfile();

			// Assert
			var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
			Assert.NotNull(notFoundResult);
			Assert.Equal("User not found.", notFoundResult.Value);
		}

		[Fact]
		public async Task GetProfile_ValidUser_ReturnsOk()
		{
			// Arrange: Thiết lập User Context với 1 User ID hợp lệ
			SetUserContext(1);
			var userDTO = new UserDTO { Id = 1, Email = "test@example.com", Name = "Test user" };
			_mockUserService.Setup(x => x.GetUserByIdAsync(1))
							.ReturnsAsync(userDTO);

			// Act
			var result = await _controller.GetProfile();

			// Assert
			var okResult = Assert.IsType<OkObjectResult>(result);
			Assert.NotNull(okResult);
			var json = JsonConvert.SerializeObject(okResult.Value);
			var response = JsonConvert.DeserializeObject<dynamic>(json);
			Assert.NotNull(response);
			int responseId = response?.Id ?? "";
			string responseEmail = response?.Email ?? "";
			string responseName = response?.Name ?? "";
			Assert.Equal(userDTO.Id, responseId);
			Assert.Equal(userDTO.Email, responseEmail);
			Assert.Equal(userDTO.Name, responseName);
		}

		[Fact]
		public async Task GetAllUsers_UserNotAuthenticated_ReturnsUnauthorized()
		{
			// Arrange: Thiết lập một HttpContext nhưng không có claims để giả lập người dùng chưa đăng nhập
			var httpContext = new DefaultHttpContext();
			httpContext.User = new ClaimsPrincipal(new ClaimsIdentity()); // Không có claim nào

			_controller.ControllerContext = new ControllerContext
			{
				HttpContext = httpContext
			};

			// Act
			var result = await _controller.GetAllUsers();

			// Assert
			var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result);
		}

		[Fact]
		public async Task GetAllUsers_NoUsers_ReturnsEemptyList()
		{
			// Arrange
			SetUserContext(1);
			_mockUserService.Setup(x => x.GetAllUsersAsync())
							.ReturnsAsync(new List<UserDTO>());

			// Act
			var result = await _controller.GetAllUsers();

			// Assert
			var okResult = Assert.IsType<OkObjectResult>(result);
			var users = Assert.IsType<List<UserDTO>>(okResult.Value);
			Assert.Empty(users);
		}

		[Fact]
		public async Task GetAllUsers_UsersExist_ReturnListOfUsers()
		{
			SetUserContext(1);
			var userList = new List<UserDTO> {
				new UserDTO { Id = 1, Email = "user1@example.com", Name = "User 1"},
				new UserDTO { Id = 2, Email = "user2@example.com", Name = "User 2" },
			};

			_mockUserService.Setup(x => x.GetAllUsersAsync())
							.ReturnsAsync(userList);

			// Act
			var result = await _controller.GetAllUsers();

			// Assert
			var okResult = Assert.IsType<OkObjectResult>(result);
			var users = Assert.IsType<List<UserDTO>>(okResult?.Value);
			Assert.NotNull(users);
			Assert.Equal(2, users.Count);
			Assert.Equal("User 1", users[0].Name);
			Assert.Equal("User 2", users[1].Name);
		}

		[Fact]
		public async Task UpdateUser_NullUpdateUserDTO_ReturnsBadRequest()
		{
			SetUserContext(1);

			// Act
			var result = await _controller.UpdateUser(1, null);

			// Assert
			var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Name or Email must be provided.", badRequestResult.Value);
		}

		[Fact]
		public async Task UpdateUser_UserNotAuthenticated_ReturnsUnauthorized()
		{
			_controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext()
			};
			var updateUserDTO = new UpdateUser { UserName = "Updated Name", Email = "update@example.com" };

			// Act
			var result = await _controller.UpdateUser(1, updateUserDTO);

			// Assert
			var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
			Assert.Equal("User ID not found in token.", unauthorizedResult.Value);
		}

		[Fact]
		public async Task UpdateUser_UserNotFoound_ReturnsNotFound()
		{
			SetUserContext(1);
			var updateUserDTO = new UpdateUser { UserName = "Updated Name", Email = "updated@example.com" };
			_mockUserService.Setup(x => x.GetUserByIdAsync(1))
							.ReturnsAsync((UserDTO?)null);

			var result = await _controller.UpdateUser(1, updateUserDTO);

			var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
			Assert.Equal("User not found.", notFoundResult.Value);
		}

		[Fact]
		public async Task UpdateUser_UserUpdatingAnotherUser_ReturnsForbid()
		{
			SetUserContext(2);
			var updateUserDTO = new UpdateUser { UserName = "Updated Name", Email = "updated@example.com" };
			_mockUserService.Setup(x => x.GetUserByIdAsync(1))
							.ReturnsAsync(new UserDTO { Id = 1, Name = "Original Name", Email = "original@example.com" });

			var result = await _controller.UpdateUser(1, updateUserDTO);

			var forbidResult = Assert.IsType<ForbidResult>(result);
		}

		[Fact]
		public async Task UpdateUSer_ValidUser_ReturnsUpdatedUser()
		{
			SetUserContext(1);
			var updateUserDTO = new UpdateUser { UserName = "Updated Name", Email = "updated@example.com" };
			var existingUser = new UserDTO { Id = 1, Name = "Original Name", Email = "original@example.com" };
			var updatedUser = new UserDTO { Id = 1, Name = "Updated Name", Email = "updated@example.com" };

			_mockUserService.Setup(x => x.GetUserByIdAsync(1))
							.ReturnsAsync(existingUser);
			_mockUserService.Setup(x => x.UpdateUserAsync(1, updateUserDTO))
							.ReturnsAsync(updatedUser);

			var result = await _controller.UpdateUser(1, updateUserDTO);

			var okResult = Assert.IsType<OkObjectResult>(result);
			Assert.NotNull(okResult.Value);
			var json = JsonConvert.SerializeObject(okResult.Value);
			var response = JsonConvert.DeserializeObject<dynamic>(json);
			Assert.NotNull(response);
			int responseId = response?.Id ?? "";
			string responseEmail = response?.Email ?? "";
			string responseName = response?.Name ?? "";
			Assert.Equal(updatedUser.Id, responseId);
			Assert.Equal(updatedUser.Email, responseEmail);
			Assert.Equal(updatedUser.Name, responseName);
		}

		[Fact]
		public async Task DeleteUser_UserrNotAuthenticated_ReturnsUnauthorized() {
			_controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext()
			};

			var result = await _controller.DeleteUser(1);

			var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
			Assert.NotNull(unauthorizedResult.Value);
			Assert.Equal("User ID not found in token.", unauthorizedResult.Value);
		}

		[Fact]
		public async Task DeleteUser_UserNotFound_ReturnsNotFound() {
			SetUserContext(1);
			_mockUserService.Setup(x => x.GetUserByIdAsync(1))
							.ReturnsAsync((UserDTO?)null);

			var result = await _controller.DeleteUser(1);

			var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
			Assert.NotNull(notFoundResult.Value);
			Assert.Equal("User not found.", notFoundResult.Value);
		}

		[Fact]
		public async Task DeleteUSer_UserDeletingAnotherUserWithoutAdminPermission_RetunsForbid() {
			SetUserContext(2);
			_mockUserService.Setup(x => x.GetUserByIdAsync(1))
							.ReturnsAsync( new UserDTO { Id = 1, Name = "Target user", Email = "target@example.com" });

			// Act
			var result = await _controller.DeleteUser(1);

			// Assert
			var forbidResult = Assert.IsType<ForbidResult>(result);
		}

		[Fact]
		public async Task DeleteUser_AdminDeletingAnotherUser_ReturnsNoContent() {
			SetUserContext(2, isAdmin: true);
			_mockUserService.Setup(x => x.GetUserByIdAsync(1))
								 .ReturnsAsync(new UserDTO { Id = 1, Name = "Target User", Email = "target@example.com" });
			_mockUserService.Setup(x => x.DeleteUserAsync(1))
							.Returns(Task.CompletedTask);

			// Act
			var result = await _controller.DeleteUser(1);

			// Assert
			Assert.IsType<NoContentResult>(result);
		}
		
		[Fact]
		public async Task DeleteUser_UserDeletingThemselves_ReturnsNoContent() {
			SetUserContext(1);
			_mockUserService.Setup(x => x.GetUserByIdAsync(1))
							.ReturnsAsync(new UserDTO { Id = 1, Name = "Self User", Email = "self@example.com" });
			_mockUserService.Setup(x => x.DeleteUserAsync(1))
							.Returns(Task.CompletedTask);

			// Act
			var result = await _controller.DeleteUser(1);

			// Assert
			Assert.IsType<NoContentResult>(result);
		}
	}
}