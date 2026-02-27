using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniCMS.Web.Data;
using MiniCMS.Web.Dtos;
using MiniCMS.Web.Models;
using MiniCMS.Web.Services;

namespace MiniCMS.Web.Controllers.Api
{
    /// <summary>
    /// Article management endpoints for the MiniCMS REST API.
    /// </summary>
    /// <remarks>
    /// COMP 4602 Assignment 1: CRUD operations for articles.
    /// HTML content is sanitized server-side to reduce script injection risk.
    /// </remarks>
    [ApiController]
    [Route("api/articles")]
    [Tags("Articles")]
    public class ArticlesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IHtmlSanitizationService _sanitizer;

        public ArticlesController(ApplicationDbContext db, IHtmlSanitizationService sanitizer)
        {
            _db = db;
            _sanitizer = sanitizer;
        }

        /// <summary>
        /// Gets all articles.
        /// </summary>
        /// <remarks>
        /// Returns article list items (ID, title, and timestamps). No pagination/search in Assignment 1.
        /// </remarks>
        /// <returns>A list of articles.</returns>
        // GET /api/articles
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ArticleListItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ArticleListItemDto>>> GetAll()
        {
            var items = await _db.Articles
                .AsNoTracking()
                .OrderByDescending(a => a.UpdatedAt)
                .ThenByDescending(a => a.CreatedAt)
                .Select(a => new ArticleListItemDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                })
                .ToListAsync();

            return Ok(items);
        }

        /// <summary>
        /// Gets a single article by ID.
        /// </summary>
        /// <param name="id">The article ID.</param>
        /// <returns>The article details if found; otherwise 404.</returns>
        // GET /api/articles/{id}
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ArticleDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ArticleDetailDto>> GetById(int id)
        {
            var article = await _db.Articles
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
                return NotFound();

            var dto = new ArticleDetailDto
            {
                Id = article.Id,
                Title = article.Title,
                Content = article.Content,
                CreatedAt = article.CreatedAt,
                UpdatedAt = article.UpdatedAt
            };

            return Ok(dto);
        }

        /// <summary>
        /// Creates a new article.
        /// </summary>
        /// <remarks>
        /// The server sanitizes HTML content before storing it.
        /// </remarks>
        /// <param name="input">Title and content for the new article.</param>
        /// <returns>The created article (detail view).</returns>
        // POST /api/articles
        [HttpPost]
        [ProducesResponseType(typeof(ArticleDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ArticleDetailDto>> Create([FromBody] ArticleCreateDto input)
        {
            // [ApiController] auto-validates, but explicit check is fine/clear
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var sanitizedContent = _sanitizer.Sanitize(input.Content);

            var article = new Article
            {
                Title = input.Title,
                Content = sanitizedContent
                // timestamps set in DbContext SaveChanges
            };

            _db.Articles.Add(article);
            await _db.SaveChangesAsync();

            var dto = new ArticleDetailDto
            {
                Id = article.Id,
                Title = article.Title,
                Content = article.Content,
                CreatedAt = article.CreatedAt,
                UpdatedAt = article.UpdatedAt
            };

            return CreatedAtAction(nameof(GetById), new { id = article.Id }, dto);
        }

        /// <summary>
        /// Updates an existing article.
        /// </summary>
        /// <remarks>
        /// The server sanitizes HTML content before saving updates.
        /// </remarks>
        /// <param name="id">The article ID.</param>
        /// <param name="input">Updated title and content.</param>
        /// <returns>204 if updated; 404 if not found; 400 if invalid.</returns>
        // PUT /api/articles/{id}
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] ArticleUpdateDto input)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var article = await _db.Articles.FirstOrDefaultAsync(a => a.Id == id);
            if (article == null)
                return NotFound();

            article.Title = input.Title;
            article.Content = _sanitizer.Sanitize(input.Content);

            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Deletes an article by ID.
        /// </summary>
        /// <param name="id">The article ID.</param>
        /// <returns>204 if deleted; 404 if not found.</returns>
        // DELETE /api/articles/{id}
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var article = await _db.Articles.FirstOrDefaultAsync(a => a.Id == id);
            if (article == null)
                return NotFound();

            _db.Articles.Remove(article);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}