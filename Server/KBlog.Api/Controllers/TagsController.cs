using KBlog.Application.Contracts.Persistence;
using KBlog.Application.DTOs.Tags;
using KBlog.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

namespace KBlog.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TagsController : ControllerBase
	{
		private readonly ITagRepository _tagRepository;
		public TagsController(ITagRepository tagRepository) {
			_tagRepository = tagRepository;
		}

		[HttpGet]
		public async Task<ActionResult<IReadOnlyList<Tag>>> GetAllTags() {
			var tags = await _tagRepository.GetAllAsync();
			return Ok(tags);
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<Tag>> CreateTag([FromBody] CreateTagDto createTagDto) {
			var existingTag = await _tagRepository.FindByNameAsync(createTagDto.Name.Trim());
			if (existingTag != null)
			{
				return BadRequest("Tag with this name already exist");
			}

			var newTag = new Tag
			{
				Name = createTagDto.Name,
				Slug = createTagDto.Name.Trim().ToLower().Replace(" ", "-"),
			};

			var createdTag = await _tagRepository.AddAsync(newTag);
			return Ok(createdTag);
		}

		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteTag(int id) {
			var tagToDelete = await _tagRepository.GetByIdAsync(id);
			if (tagToDelete == null)
			{
				return NotFound();
			}

			await _tagRepository.DeleteAsync(tagToDelete);
			return NoContent(); // Trả về 204 No Conent khi xóa thành công
		}
	}
}
