using KBlog.DTOs;
using KBlog.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace KBlogTest.Controllers
{
	public class PostControllerTest
	{
		private readonly Mock<IPostService> _mockPostService;
		private readonly PostsController _controller;
		public PostControllerTest() {
			_mockPostService = new Mock<IPostService>();
			_controller = new PostsController(_mockPostService.Object);
		}

		[Fact]
		public async Task CreatetPost_ReturnsCreatedResult_WhenPostIsValid()
		{
			// Arrange
			var createPostDTO = new CreatePostDTO
			{
				Title = "Test create Blog Post",
				Content = "Test creating blog post api",
				userId = 0,
			};

			var postDTO = new PostDTO
			{
				Id = 0,
				Title = "Test create Blog Post",
				Content = "Test creating blog post api",
				CreatedAt = DateTime.UtcNow,
				UserId = 0,
			};

			_mockPostService.Setup(s => s.CreatePostAsync(createPostDTO))
							.ReturnsAsync(postDTO);

			// Act
			var result = await _controller.CreatePost(createPostDTO);

			// Assert
			var createdResult = Assert.IsType<CreatedAtActionResult>(result);
			var returnedPost = Assert.IsType<PostDTO>(createdResult.Value);

			Assert.Equal(postDTO.Id, returnedPost.Id);
			Assert.Equal(postDTO.Title, returnedPost.Title);
			Assert.Equal(postDTO.Content, returnedPost.Content);
			Assert.Equal(postDTO.UserId, returnedPost.UserId);
		}

		[Fact]
		public async Task CreatePost_ReturnsBadRequest_WhenPostDTOIsNull() {
			// Arrange 
			CreatePostDTO? createPostDTO = null;

			// Act
			var result = await _controller.CreatePost(createPostDTO!);

			// Assert
			var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Post data cannot be null.", badRequestResult.Value);
		}

		[Fact]
		public async Task GetAllPosts_ReturnsBadRequest_WhenPageIsLessThanOne() {
			// Arrange
			int invalidPage = 0;

			// Act
			var result = await _controller.GetAllPosts(invalidPage);

			// Assert
			var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Page number must be greater than 0.", badRequestResult?.Value);
		}

		[Fact]
		public async Task GetAllPosts_ReturnsOk_WithListOfPosts()
		{
			// Arrange
			int validPage = 1;
			var mockPosts = new List<PostDTO>()
			{
				new PostDTO() { Id = 1, Title = "First Post", Content = "Content of the first post", CreatedAt = DateTime.UtcNow },
				new PostDTO() { Id = 2, Title = "Second Post", Content = "Content of the second post", CreatedAt = DateTime.UtcNow },
			};

			_mockPostService.Setup(s => s.GetAllPostAsync(validPage)).ReturnsAsync(mockPosts);

			// Act
			var result = await _controller.GetAllPosts(validPage);

			// Assert
			var okResult = Assert.IsType<OkObjectResult>(result);
			var returnedPosts = Assert.IsType<List<PostDTO>>(okResult.Value);
			Assert.Equal(mockPosts.Count, returnedPosts.Count);
			Assert.Equal(mockPosts[0].Id, returnedPosts[0].Id);
			Assert.Equal(mockPosts[0].Title, returnedPosts[0].Title);
			Assert.Equal(mockPosts[1].Id, returnedPosts[1].Id);
			Assert.Equal(mockPosts[1].Title, returnedPosts[1].Title);
		}

		[Fact]
		public async Task GetAllPosts_ReturnsOk_WithEmptyList_WhenNoPostsExist() {
			// Arrange
			int validPage = 1;
			var mockEmptyPosts = new List<PostDTO>();
			_mockPostService.Setup(s => s.GetAllPostAsync(validPage)).ReturnsAsync(mockEmptyPosts);

			// Act 
			var result = await _controller.GetAllPosts(validPage);

			// Assert
			var okResult = Assert.IsType<OkObjectResult>(result);
			var returnedPosts = Assert.IsType<List<PostDTO>>(okResult.Value);
			Assert.Empty(returnedPosts);
		}

		[Fact]
		public async Task GetPostById_ReturnsNotFound_WhenPostDoesNotExist() {
			// Arrange
			int nonExistentPostId = 999;
			_mockPostService.Setup(s => s.GetPostByIdAsync(nonExistentPostId))
							.ReturnsAsync((PostDTO?)null);

			// Act
			var result = await _controller.GetPostById(nonExistentPostId);

			// Assert
			var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
			Assert.Equal($"Post with ID {nonExistentPostId} not found.", notFoundResult.Value);
		}

		[Fact]
		public async Task GetPostById_ReturnsOk_WhenPostExists()
		{
			// Arrange
			int existingPostId = 1;
			var mockPost = new PostDTO
			{
				Id = existingPostId,
				Title = "Test Post",
				Content = "This is a test post",
				CreatedAt = DateTime.UtcNow,
				UserId = 1,
				UserName = "Test User",
			};
			_mockPostService.Setup(s => s.GetPostByIdAsync(existingPostId))
							.ReturnsAsync(mockPost);

			// Act
			var result = await _controller.GetPostById(existingPostId);

			// Assert
			var okResult = Assert.IsType<OkObjectResult>(result);
			var returnedPost = Assert.IsType<PostDTO>(okResult.Value);
			Assert.Equal(mockPost.Id, returnedPost.Id);
			Assert.Equal(mockPost.Title, returnedPost.Title);
			Assert.Equal(mockPost.Content, returnedPost.Content);
			Assert.Equal(mockPost.UserId, returnedPost.UserId);
			Assert.Equal(mockPost.UserName, returnedPost.UserName);
		}

		[Fact]
		public async Task UpdatePost_ReturnsBadRequest_WhenPostDTOIsNull() {
			// Arrange
			int validId = 1;
			UpdatePostDTO? updatePostDTO = null;

			// Act
			var result = await _controller.UpdatePost(validId, updatePostDTO!);

			// Assert
			var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Post data cannot be null.", badRequestResult.Value);
		}

		[Fact]
		public async Task UpdatePost_ReturnsNotFound_WhenPostDoestNotExit() {
			// Arrange
			int nonExistentId = 999;
			var updatePostDTO = new UpdatePostDTO
			{
				Title = "Updated Title",
				Content = "Updated Content",
			};
			_mockPostService.Setup(s => s.UpdatePostAsync(nonExistentId, updatePostDTO))
							.ReturnsAsync((PostDTO?)null);

			// Act
			var result = await _controller.UpdatePost(nonExistentId, updatePostDTO);

			// Assert
			var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
			Assert.Equal($"Post with ID {nonExistentId} not found.", notFoundResult.Value);
		}

		[Fact]
		public async Task UpdatePost_ReturnsOk_WhenPostIsUpdatedSuccessfully() {
			var existingId = 1;
			var updatePostDTO = new UpdatePostDTO
			{
				Title = "Updated Title",
				Content = "Updated Content",
			};

			var updatedPostDTO = new PostDTO
			{
				Id = existingId,
				Title = "Updated Title",
				Content = "Updated Content",
				UpdatedAt = DateTime.UtcNow,
			};
			_mockPostService.Setup(s => s.UpdatePostAsync(existingId, updatePostDTO))
							.ReturnsAsync(updatedPostDTO);

			// Act
			var result = await _controller.UpdatePost(existingId, updatePostDTO);

			// Assert
			var okResult = Assert.IsType<OkObjectResult>(result);
			var returnedPost = Assert.IsType<PostDTO>(okResult.Value);

			Assert.Equal(updatedPostDTO.Id, returnedPost.Id);
			Assert.Equal(updatedPostDTO.Title, returnedPost.Title);
			Assert.Equal(updatedPostDTO.Content, returnedPost.Content);
			Assert.Equal(updatedPostDTO.UpdatedAt, returnedPost.UpdatedAt);
		}

		[Fact]
		public async Task DeletePost_ReturnsNotFound_WhenPostDoesNotExist() {
			// Arrange
			int nonExistentId = 999;

			_mockPostService.Setup(s => s.DeletePostAsync(nonExistentId))
							.ReturnsAsync(false); // Giả lập ko tìm thấy bài post

			// Act
			var result = await _controller.DeletePost(nonExistentId);

			// Assert
			var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
			Assert.Equal($"Post with ID {nonExistentId} not found.", notFoundResult.Value);
		}

		[Fact]
		public async Task DeletePost_ReturnsNoContent_WhenPostIsDeletedSuccessfully() {
			// Arrange
			int existingId = 1;
			_mockPostService.Setup(s => s.DeletePostAsync(existingId))
							.ReturnsAsync(true);

			// Act
			var result = await _controller.DeletePost(existingId);

			// Assert
			Assert.IsType<NoContentResult	>(result);
		}
	}
}
