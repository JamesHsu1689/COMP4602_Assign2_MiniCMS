using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "Admin,Writer")]
    public class ArticlesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IHtmlSanitizationService _sanitizer;

        public ArticlesController(ApplicationDbContext db, IHtmlSanitizationService sanitizer)
        {
            _db = db;
            _sanitizer = sanitizer;
        }

        private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);
        private bool IsAdmin => User.IsInRole("Admin");

        /// <summary>
        /// Gets all public article list items for the Blazor frontend.
        /// </summary>
        /// <remarks>
        /// Anonymous, read-only endpoint.
        /// Returns all articles ordered by latest first.
        /// </remarks>
        /// <returns>A public list of all articles.</returns>
        // GET /api/articles/public
        [HttpGet("public")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<ArticleListItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ArticleListItemDto>>> GetPublic()
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
        /// Gets all articles.
        /// </summary>
        /// <remarks>
        /// Returns article list items (ID, title, and timestamps).
        /// Admin sees all articles; Writer sees only their own.
        /// </remarks>
        /// <returns>A list of articles.</returns>
        // GET /api/articles
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ArticleListItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ArticleListItemDto>>> GetAll()
        {
            var query = _db.Articles.AsNoTracking();

            if (!IsAdmin)
            {
                var userId = CurrentUserId;
                query = query.Where(a => a.UserId == userId);
            }

            var items = await query
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
            var query = _db.Articles
                .AsNoTracking()
                .Where(a => a.Id == id);

            if (!IsAdmin)
            {
                var userId = CurrentUserId;
                query = query.Where(a => a.UserId == userId);
            }

            var article = await query.FirstOrDefaultAsync();

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
        /// The created article is assigned to the current user.
        /// </remarks>
        /// <param name="input">Title and content for the new article.</param>
        /// <returns>The created article (detail view).</returns>
        // POST /api/articles
        [HttpPost]
        [ProducesResponseType(typeof(ArticleDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ArticleDetailDto>> Create([FromBody] ArticleCreateDto input)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var sanitizedContent = _sanitizer.Sanitize(input.Content);
            var userId = CurrentUserId;

            var article = new Article
            {
                Title = input.Title,
                Content = sanitizedContent,
                UserId = userId
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
        /// Admin can update any article; Writer can update only their own.
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

            var query = _db.Articles.Where(a => a.Id == id);

            if (!IsAdmin)
            {
                var userId = CurrentUserId;
                query = query.Where(a => a.UserId == userId);
            }

            var article = await query.FirstOrDefaultAsync();
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
            var query = _db.Articles.Where(a => a.Id == id);

            if (!IsAdmin)
            {
                var userId = CurrentUserId;
                query = query.Where(a => a.UserId == userId);
            }

            var article = await query.FirstOrDefaultAsync();
            if (article == null)
                return NotFound();

            _db.Articles.Remove(article);
            await _db.SaveChangesAsync();

            return NoContent();
        }


        // GET /api/articles/public/{id}
        // This endpoint allows anonymous users to get article details by ID.
        // It is used by the Blazor frontend to display article details without authentication.
        // Returns 404 if the article does not exist, otherwise returns the article details.
        // This endpoint does not require authentication and is accessible to the public.
        // Note: This is a separate endpoint from the authenticated GET /api/articles/{id} to allow anonymous access while keeping the main GET /api/articles/{id} protected.
        // The public GET endpoint returns the same article details as the authenticated GET endpoint, but it does not check for ownership or roles since it is accessible to everyone.
        // The public GET endpoint is intended for displaying article details on the Blazor frontend without requiring users to log in, while the authenticated GET endpoint is intended for use in the admin interface where authentication and authorization are required.
        [HttpGet("public/{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ArticleDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ArticleDetailDto>> GetPublicById(int id)
        {
            var article = await _db.Articles
                .AsNoTracking()
                .Where(a => a.Id == id)
                .Select(a => new ArticleDetailDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Content = a.Content,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (article == null)
                return NotFound();

            return Ok(article);
        }
    }
}