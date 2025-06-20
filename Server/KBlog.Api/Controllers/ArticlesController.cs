using KBlog.Application.Contracts.Persistence;
using KBlog.Application.DTOs.Article;
using KBlog.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slugify;
using System.IdentityModel.Tokens.Jwt;
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

		[HttpGet]
		public async Task<ActionResult<IReadOnlyList<ArticleDto>>> GetAllArticles() {
			var articles = await _articleRepository.GetAllAsync();

			// convert List<Article> tô List<ArticleDto> to return to client
			var articleDtos = articles.Select(article => new ArticleDto
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
					.Select(name => name!).ToList(),
			}).ToList();

			return Ok(articleDtos);
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
	}
}
