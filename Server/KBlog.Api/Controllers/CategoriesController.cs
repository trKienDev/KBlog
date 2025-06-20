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
	}
}
