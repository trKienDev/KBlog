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

		[HttpGet("{id}")]
		public async Task<ActionResult<Tag>> GetTagById(int id)
		{
			var tag = await _tagRepository.GetByIdAsync(id);
			if (tag == null)
			{
				return NotFound();
			}

			return Ok(tag);
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

		[HttpPut("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UpdateTag(int tag_id, [FromBody] CreateTagDto tag_dto)
		{
			var tagToUpdate = await _tagRepository.GetByIdAsync(tag_id);
			if (tagToUpdate == null)
			{
				return NotFound();
			}

			// Kiểm tra xem tên mới có bị trùng với 1 tag khác ko
			var existingTagWithNewName = await _tagRepository.FindByNameAsync(tag_dto.Name.Trim());
			if (existingTagWithNewName != null && existingTagWithNewName.Id != tag_id)
			{
				return Conflict("Another tag with this name already exist");
			}

			tagToUpdate.Name = tag_dto.Name.Trim();
			tagToUpdate.Slug = tag_dto.Name.Trim().ToLower().Replace(" ", "-");
			await _tagRepository.UpdateAsync(tagToUpdate);

			return NoContent();
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
