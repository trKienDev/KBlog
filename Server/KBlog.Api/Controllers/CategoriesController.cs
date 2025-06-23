using KBlog.Application.Contracts.Persistence;
using KBlog.Application.DTOs.Category;
using KBlog.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KBlog.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CategoriesController : ControllerBase
	{
		private readonly ICategoryRepository _categoryRepository;
		public CategoriesController(ICategoryRepository categoryRepository)
		{
			_categoryRepository = categoryRepository;
		}

		// GET: api/Categories
		[HttpGet]
		public async Task<ActionResult<IReadOnlyList<Category>>> GetAllCategories()
		{
			var categories = await _categoryRepository.GetAllAsync();
			return Ok(categories);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<Category>> GetCategoryById(int id)
		{
			var category = await _categoryRepository.GetByIdAsync(id);
			if (category == null)
			{
				return NotFound();
			}

			return Ok(category);
		}

		// POST: api/Categories
		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<Category>> CreateCategory([FromBody] CreateCategoryDto createCategoryDto) {
			var newCategory = new Category
			{
				Name = createCategoryDto.Name,
				Description = createCategoryDto.Description,
				Slug = createCategoryDto.Name.Trim().ToLower().Replace(" ", "-")
			};

			var createdCategory = await _categoryRepository.AddAsync(newCategory);
			return Ok(createdCategory);
		}

		[HttpPut("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UpdateCategory(int id, [FromBody] CreateCategoryDto categoryDto) {
			var categoryToUpdate = await _categoryRepository.GetByIdAsync(id);
			if (categoryToUpdate == null) {
				return NotFound();	
			}

			categoryToUpdate.Name = categoryDto.Name;
			categoryToUpdate.Description = categoryDto.Description;
			categoryToUpdate.Slug  = categoryDto.Name.Trim().ToLower().Replace(" ", "-");
			await _categoryRepository.UpdateAsync(categoryToUpdate);

			return NoContent();
		}

		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteCategory(int id)
		{
			var categoryToDelete = await _categoryRepository.GetByIdAsync(id);
			if (categoryToDelete == null)
			{
				return NotFound();
			}
			
			await _categoryRepository.DeleteAsync(categoryToDelete);
			return NoContent();
		}
	}
}
