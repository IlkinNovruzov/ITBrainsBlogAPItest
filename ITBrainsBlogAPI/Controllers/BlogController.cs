using ITBrainsBlogAPI.Models;
using ITBrainsBlogAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITBrainsBlogAPI.Services;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Reflection.Metadata;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace ITBrainsBlogAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WeatherForecastController> _logger;
        public AzureBlobService _service;
        private readonly TokenService _tokenService;

        public BlogController(AppDbContext context, UserManager<AppUser> userManager, IConfiguration configuration, ILogger<WeatherForecastController> logger, AzureBlobService service, TokenService tokenService)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
            _service = service;
            _tokenService = tokenService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Blog>>> GetBlogs()
        {

            var blogs = await _context.Blogs
                .Include(b => b.Images)
                .Include(b => b.Reviews)
                .Include(b => b.AppUser)
                .Include(b => b.Likes)
                .OrderByDescending(b => b.Likes.Count)
                .ToListAsync();
            return Ok(blogs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Blog>> GetBlog([FromRoute] int id)
        {
            var blog = await _context.Blogs
                .Include(b => b.Images)
                .Include(b => b.Reviews.Where(r => r.ParentReviewId == null))
                .ThenInclude(r => r.ParentReview)
                .Include(b => b.AppUser)
                .Include(b => b.Likes)
                .SingleOrDefaultAsync(b => b.Id == id);

            if (blog == null)
            {
                return NotFound();
            }

            return blog;
        }
        [HttpPost("create")]
        public async Task<ActionResult<Blog>> AddBlog([FromForm] BlogDTO model, [FromHeader(Name = "Authorization")] string token)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _tokenService.ValidateTokenAndGetUserAsync(token);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            try
            {
                var transaction = await _context.Database.BeginTransactionAsync();
                var blog = new Blog
                {
                    AppUserId = user.Id,
                    Title = model.Title,
                    Body = model.Body,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                _context.Blogs.Add(blog);
                await _context.SaveChangesAsync();

                foreach (var item in model.ImgFiles ?? Enumerable.Empty<IFormFile>())
                {
                    if (!FileExtensions.IsImage(item))
                    {
                        return BadRequest("This file type is not accepted.");
                    }

                    var imgUrl = await _service.UploadFile(item);
                    var imageUrl = $"https://itbblogstorage.blob.core.windows.net/itbcontainer/{imgUrl}";

                    var img = new Image
                    {
                        BlogId = blog.Id,
                        ImageUrl = imageUrl
                    };

                    _context.Images.Add(img);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok("Blog Added.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the blog.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("edit/{blogId}")]
        public async Task<ActionResult<Blog>> EditBlog([FromRoute] int blogId, [FromForm] BlogDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized("Please login to edit a blog.");
            }

            var existingBlog = await _context.Blogs.FindAsync(blogId);
            if (existingBlog == null)
            {
                return NotFound("Blog not found.");
            }

            if (existingBlog.AppUserId != user.Id)
            {
                return Unauthorized("You do not have permission to edit this blog.");
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    existingBlog.Title = model.Title;
                    existingBlog.Body = model.Body;
                    existingBlog.UpdatedAt = DateTime.UtcNow;

                    _context.Entry(existingBlog).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    // Delete existing images associated with the blog
                    var existingImages = _context.Images.Where(i => i.BlogId == existingBlog.Id);
                    _context.Images.RemoveRange(existingImages);

                    // Add new images
                    foreach (var item in model.ImgFiles ?? Enumerable.Empty<IFormFile>())
                    {
                        if (!FileExtensions.IsImage(item))
                        {
                            return BadRequest("This file type is not accepted.");
                        }

                        var imgUrl = await _service.UploadFile(item);
                        var imageUrl = $"https://itbblogstorage.blob.core.windows.net/blogcontainer/{imgUrl}";

                        var img = new Image
                        {
                            BlogId = existingBlog.Id,
                            ImageUrl = imageUrl
                        };

                        _context.Images.Add(img);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok("Blog updated successfully.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "An error occurred while editing the blog.");
                    return StatusCode(500, "Internal server error");
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlog([FromRoute] int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }

            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();

            return Ok("Removed");
        }

        [HttpGet("{id}/userblogs")]
        public async Task<IActionResult> GetUserBlogs([FromRoute] int id)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            var blogs = await _context.Blogs
               .Include(b => b.Images)
               .Include(b => b.Reviews)
               .Include(b => b.Likes)
               .Where(b => b.AppUserId == user.Id)
               .OrderByDescending(b => b.CreatedAt)
               .ToListAsync();
            return Ok(blogs);
        }

        private bool BlogExists(int id)
        {
            return _context.Blogs.Any(e => e.Id == id);
        } 

        #region Like
        [HttpPost("like/{blogId}")]
        public async Task<ActionResult> LikeBlog([FromRoute] int blogId, [FromHeader(Name = "Authorization")] string token)
        {
            var user = await _tokenService.ValidateTokenAndGetUserAsync(token);
            if (user == null)
            {
                return Unauthorized("Invalid token or user.");
            }

            var blog = await _context.Blogs.Include(b => b.Likes).FirstOrDefaultAsync(b => b.Id == blogId);
            if (blog == null)
            {
                return NotFound("Blog not found.");
            }

            var existingLike = blog.Likes.FirstOrDefault(l => l.AppUserId == user.Id);

            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike);
                await _context.SaveChangesAsync();
                return Ok(blog);
            }
            else
            {
                var like = new Like
                {
                    BlogId = blog.Id,
                    AppUserId = user.Id
                };

                _context.Likes.Add(like);
                await _context.SaveChangesAsync();
                return Ok(blog);
            }
        }

        [HttpGet("is-liked")]
        public async Task<IActionResult> IsLiked(int userId, int blogId)
        {
            var like = await _context.Likes.FirstOrDefaultAsync(l => l.AppUserId == userId && l.BlogId == blogId);

            return Ok(like != null);
        }

        [HttpGet("liked-blogs")]
        public async Task<IActionResult> GetLikedBlogs([FromHeader(Name = "Authorization")] string token)
        {
            var user = await _tokenService.ValidateTokenAndGetUserAsync(token);
            if (user == null) return Unauthorized();

            var likedBlogs = await _context.Likes
                .Where(l => l.AppUserId == user.Id)
                .Include(l => l.Blog)
                .ToListAsync();

            return Ok(likedBlogs);
        }

        [HttpGet("blog/{blogId}/likes")]
        public async Task<IActionResult> GetBlogLikes([FromRoute] int blogId, [FromHeader(Name = "Authorization")] string token)
        {
            var user = await _tokenService.ValidateTokenAndGetUserAsync(token);
            if (user == null) return Unauthorized();

            var blogLikes = await _context.Likes
                .Where(l => l.BlogId == blogId)
                .Include(l => l.AppUser)
                .ToListAsync();

            var blogLikesDto = blogLikes.Select(l => new
            {
                UserId = l.AppUser.Id,
                Name = l.AppUser.Name,
                UserEmail = l.AppUser.Email,
            });

            return Ok(blogLikesDto);
        }
        #endregion

        #region Review
        [HttpPost("add-review")]
        public async Task<ActionResult> AddReviewBlog([FromBody] ReviewDTO model, [FromHeader(Name = "Authorization")] string token)
        {
            var user = await _tokenService.ValidateTokenAndGetUserAsync(token);
            if (user == null)
            {
                return Unauthorized("Invalid token or user.");
            }

            var blog = await _context.Blogs.FindAsync(model.BlogId);
            if (blog == null)
            {
                return NotFound("Blog not found.");
            }

            Review? parentReview = null;
            if (model.ParentReviewId.HasValue)
            {
                parentReview = await _context.Reviews.FindAsync(model.ParentReviewId.Value);
                if (parentReview == null)
                {
                    return NotFound("Parent review not found.");
                }
            }

            var review = new Review
            {
                AppUserId = user.Id,
                BlogId = blog.Id,
                Comment = model.Comment,
                Date = DateTime.UtcNow,
                ParentReviewId = model.ParentReviewId,
                ParentReview = parentReview
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Ok("Review added.");
        }

        [HttpPut("edit-review/{reviewId}")]
        public async Task<ActionResult> EditReview([FromRoute] int reviewId, [FromBody] ReviewDTO model, [FromHeader(Name = "Authorization")] string token)
        {
            var user = await _tokenService.ValidateTokenAndGetUserAsync(token);
            if (user == null)
            {
                return Unauthorized("Invalid token or user.");
            }

            var existingReview = await _context.Reviews.Include(r => r.Blog).FirstOrDefaultAsync(r => r.Id == reviewId);
            if (existingReview == null)
            {
                return NotFound("Review not found.");
            }

            if (existingReview.AppUserId != user.Id)
            {
                return Unauthorized("You do not have permission to edit this review.");
            }

            existingReview.Comment = model.Comment;

            if (model.ParentReviewId.HasValue)
            {
                var parentReview = await _context.Reviews.FindAsync(model.ParentReviewId.Value);
                if (parentReview == null)
                {
                    return NotFound("Parent review not found.");
                }
                existingReview.ParentReviewId = model.ParentReviewId;
                existingReview.ParentReview = parentReview;
            }
            else
            {
                existingReview.ParentReviewId = null;
                existingReview.ParentReview = null;
            }

            _context.Reviews.Update(existingReview);
            await _context.SaveChangesAsync();

            return Ok("Review updated.");
        }

        [HttpDelete("delete-review/{reviewId}")]
        public async Task<ActionResult> DeleteReview([FromRoute] int reviewId, [FromHeader(Name = "Authorization")] string token)
        {
            var user = await _tokenService.ValidateTokenAndGetUserAsync(token);
            if (user == null)
            {
                return Unauthorized("Invalid token or user.");
            }

            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null)
            {
                return NotFound("Review not found.");
            }

            if (review.AppUserId != user.Id)
            {
                return Unauthorized("You do not have permission to delete this review.");
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Ok("Review deleted.");
        }
        #endregion

        #region SaveBlog
        [HttpPost("save")]
        public async Task<IActionResult> SaveBlog([FromRoute] int blogId, [FromHeader(Name = "Authorization")] string token)
        {
            var user = await _tokenService.ValidateTokenAndGetUserAsync(token);
            if (user == null) return Unauthorized();
            await _context.SavedBlogs
        }

        #endregion


    }
}
