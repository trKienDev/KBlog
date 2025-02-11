using KBlog.DTOs;
using KBlog.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
	private readonly IPostService _postService;

	public PostsController(IPostService postService)
	{
		_postService = postService;
	}

	[HttpPost]
	public async Task<IActionResult> CreatePost([FromBody] CreatePostDTO postDto)
	{
		if (postDto == null)
		{
			return BadRequest("Post data cannot be null.");
		}

		var result = await _postService.CreatePostAsync(postDto);

		return CreatedAtAction(nameof(GetPostById), new { id = result.Id }, result);
	}

	// Lấy danh sách bài viết (với phân trang)
	[HttpGet]
	public async Task<IActionResult> GetAllPosts([FromQuery] int page = 1)
	{
		if (page < 1)
		{
			return BadRequest("Page number must be greater than 0.");
		}

		var posts = await _postService.GetAllPostAsync(page);
		return Ok(posts);
	}

	// Lấy chi tiết bài viết theo ID
	[HttpGet("{id}")]
	public async Task<IActionResult> GetPostById(int id)
	{
		var post = await _postService.GetPostByIdAsync(id);
		if (post == null)
		{
			return NotFound($"Post with ID {id} not found.");
		}

		return Ok(post);
	}

	// Cập nhật bài viết
	[HttpPut("{id}")]
	public async Task<IActionResult> UpdatePost(int id, [FromBody] UpdatePostDTO postDto)
	{
		if (postDto == null)
		{
			return BadRequest("Post data cannot be null.");
		}

		var updatedPost = await _postService.UpdatePostAsync(id, postDto);
		if (updatedPost == null)
		{
			return NotFound($"Post with ID {id} not found.");
		}

		return Ok(updatedPost);
	}

	// Xóa bài viết
	[HttpDelete("{id}")]
	public async Task<IActionResult> DeletePost(int id)
	{
		var success = await _postService.DeletePostAsync(id);
		if (!success)
		{
			return NotFound($"Post with ID {id} not found.");
		}

		return NoContent(); // Xóa thành công, không trả về nội dung.
	}
}
