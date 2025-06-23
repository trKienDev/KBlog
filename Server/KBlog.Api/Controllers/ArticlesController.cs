using KBlog.Application.Contracts.Persistence;
using KBlog.Application.DTOs.Article;
using KBlog.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slugify;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices;
using System.Security.Claims;

namespace KBlog.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ArticlesController : ControllerBase
	{
		private readonly IArticleRepository _articleRepository;
		private readonly ITagRepository _tagRepository;
		public ArticlesController(
			IArticleRepository articleRepository
			, ITagRepository tagRepository) {
			_articleRepository = articleRepository;
			_tagRepository = tagRepository;
		}

		[HttpGet("all")]
		public async Task<ActionResult<IReadOnlyList<ArticleDto>>> GetAllArticles() {
			var articles = await _articleRepository.GetAllAsync();
			var article_dtos = MappingArticlesDto(articles);
			return Ok(article_dtos);
		}

		[HttpGet]
		public async Task<ActionResult<IReadOnlyList<ArticleDto>>> GetPagedArticles(
			[FromQuery] int page_number = 1, 
			[FromQuery] int page_size = 10
		) {
			var articles = await _articleRepository.GetPagedAsync(page_number, page_size);
			var aticles_dtos = MappingArticlesDto(articles);
			return Ok(aticles_dtos);
		}

		[HttpGet("{slug}")]
		public async Task<ActionResult<ArticleDto>> GetArticleBySlug(string slug)
		{
			var article = await _articleRepository.GetBySlugAsync(slug);
			if (article == null)
			{
				return NotFound();
			}

			var articleDto = new ArticleDto
			{
				Id = article.Id,
				Title = article.Title,
				Slug = article.Slug,
				Content = article.Content,
				Excerpt = article.Excerpt,
				CoverImageUrl = article.CoverImageUrl,
				CreatedAt = article.CreatedAt,
				AuthorName = article.Author?.UserName ?? "N/A",

				// Lấy danh sách tên các category & tag
				Categories = article.ArticleCategories
					.Select(ac => ac.Category?.Name)
					.Where(name => name != null)
					.Select(name => name!)
					.ToList(),

				Tags = article.ArticleTags
					.Where(at => at.Tag != null)
					.Select(at => at.Tag!.Name).ToList()
			};

			return Ok(articleDto);
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> CreateArticle([FromBody] CreateArticleDto createArticleDto) {
			// Get user id from token
			var authorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (authorId == null)
			{
				return Unauthorized();
			}

			// Tạo slug tối ưu từ file
			var slugHelper = new SlugHelper();
			var slug = slugHelper.GenerateSlug(createArticleDto.Title);
			if(await _articleRepository.IsSlugExistAsync(slug)) {
				return Conflict("Article title has been existed!");
			}

			var newArticle = new Article
			{
				Title = createArticleDto.Title,
				Slug = slug,
				Content = createArticleDto.Content,
				Excerpt = createArticleDto.Excerpt,
				CoverImageUrl = createArticleDto.CoverImageUrl,
				CreatedAt = DateTime.UtcNow,
				AuthorId = authorId,
				Status = Domain.Entities.PostStatus.Published, // Mặc định là published
			};

			// 1. Xử lý Categories
			if (createArticleDto.CategoryIds != null && createArticleDto.CategoryIds.Any())
			{
				foreach(var categoryId in createArticleDto.CategoryIds) {
					// Chỉ cần tạo bản ghi trong bảng nối --> EF Core sẽ tự hiểu
					newArticle.ArticleCategories.Add(new ArticleCategory { CategoryId = categoryId });
				}
			}
			// 2. Xử lý Tags (tìm hoặc tạo mới)
			if (createArticleDto.Tags != null && createArticleDto.Tags.Any())
			{
				foreach (var tagName in createArticleDto.Tags)
				{
					var normalizedTagName = tagName.Trim();
					var existingTag = await _tagRepository.FindByNameAsync(normalizedTagName);

					if (existingTag != null)
					{
						// Nếu tag đã tồn tại --> chỉ cần tạo liên kết
						newArticle.ArticleTags.Add(new ArticleTag { TagId = existingTag.Id });
					}
					else
					{
						// Nếu tag chưa tồn tại --> tạo tag mới & tạo liên kết
						var newTag = new Tag { Name = normalizedTagName, Slug = normalizedTagName.ToLower() };
						newArticle.ArticleTags.Add(new ArticleTag { Tag = newTag });
					}
				}
			}

			// AddAsync sẽ lưu Article & tất cả liên kết ArticleCategory, ArticleTag và cả Tag mới
			var createdArticle = await _articleRepository.AddAsync(newArticle);

			var articleToReturn = new ArticleDto { 
				Id = createdArticle.Id, 
				Title = createdArticle.Title,
				Slug = createdArticle.Slug,
			};

			return CreatedAtAction(nameof(GetArticleBySlug), new { slug = createdArticle.Slug }, articleToReturn);
		}

		[HttpPut("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UpdateArticle(int id, [FromBody] CreateArticleDto updateArticleDto) {
			var articleToUpdate = await _articleRepository.GetByIdAsync(id);	
			if(articleToUpdate == null) {
				return NotFound("Article not found");
			}

			articleToUpdate.Title = updateArticleDto.Title;
			articleToUpdate.Content = updateArticleDto.Content;
			articleToUpdate.Excerpt = updateArticleDto.Excerpt;
			articleToUpdate.CoverImageUrl = updateArticleDto.CoverImageUrl;
			articleToUpdate.UpdatedAt = DateTime.UtcNow;

			// Xử lý logic cập nhật slug một cách an toàn nếu title thay đổi.
			var slug_helper = new SlugHelper();
			var new_slug = slug_helper.GenerateSlug(updateArticleDto.Title);
			if (articleToUpdate.Slug != new_slug)
			{
				if (await _articleRepository.IsSlugExistAsync(new_slug, id))
				{
					return Conflict("The Article title has been existed");
				}
				articleToUpdate.Slug = new_slug;
			}

			// TODO: Xử lý logic cập nhật Categories và Tags.
			// 1. Xóa tất cả liên kết Category cũ
			articleToUpdate.ArticleCategories.Clear();
			// 2. Thêm lại các liên kết Category mới từ DTO
			if(updateArticleDto.CategoryIds != null) {
				foreach (var categoryId in updateArticleDto.CategoryIds)
				{
					articleToUpdate.ArticleCategories.Add(new ArticleCategory { CategoryId = categoryId });
				}				
			}
			// 3. Xóa tất cả liên kết Tag cũ
			articleToUpdate.ArticleTags.Clear();
			// 4. Thêm lại các liên kết Tag mới từ DTO (bao gồm logic tìm hoặc tạo mới)
			if (updateArticleDto.Tags != null)
			{
				foreach (var tagName in updateArticleDto.Tags)
				{
					var normalizedTagName = tagName.Trim();
					var existingTag = await _tagRepository.FindByNameAsync(normalizedTagName);
					if (existingTag != null)
					{
						articleToUpdate.ArticleTags.Add(new ArticleTag { TagId = existingTag.Id });
					}
					else
					{
						var newTag = new Tag { Name = normalizedTagName, Slug = normalizedTagName.ToLower() };
						articleToUpdate.ArticleTags.Add(new ArticleTag { Tag = newTag });
					}
				}
			}
			 
			await _articleRepository.UpdateAsync(articleToUpdate);
			return NoContent();
		}

		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteArticle(int id) {
			var articleToDelete = await _articleRepository.GetByIdAsync(id);
			if (articleToDelete == null)
			{
				return NotFound();
			}

			await _articleRepository.DeleteAsync(articleToDelete);
			return NoContent();
		}

		private List<ArticleDto> MappingArticlesDto(IEnumerable<Article> articles)
		{
			return articles.Select(article => new ArticleDto
			{
				Id = article.Id,
				Title = article.Title,
				Slug = article.Slug,
				Excerpt = article.Excerpt,
				CoverImageUrl = article.CoverImageUrl,
				CreatedAt = article.CreatedAt,
				AuthorName = article.Author?.UserName ?? "N/A",
				Categories = article.ArticleCategories.Select(ac => ac.Category?.Name)
					.Where(name => name != null)
					.Select(name => name!).ToList(),
				Tags = article.ArticleTags.Select(at => at.Tag?.Name)
					.Where(name => name != null)
					.Select(name => name!).ToList()
			}).ToList();
		}
	}
}
