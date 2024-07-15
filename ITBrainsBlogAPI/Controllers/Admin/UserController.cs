using ITBrainsBlogAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITBrainsBlogAPI.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly AppDbContext _context;

        public UserController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<IdentityUser>>> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            return Ok(users);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] int id)
        {
            var user = await _userManager.Users
                .Include(u => u.Reviews)
                .Include(u => u.Blogs)
                .Include(u => u.Likes)
                .Include(u => u.RefreshTokens)
                .Include(u => u.SavedBlogs)
                .SingleOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new { Message = "User Not Found" });
            }

            _context.Reviews.RemoveRange(user.Reviews);
            _context.Blogs.RemoveRange(user.Blogs);
            _context.Likes.RemoveRange(user.Likes);
            _context.RefreshTokens.RemoveRange(user.RefreshTokens);
            _context.SavedBlogs.RemoveRange(user.SavedBlogs);

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            await _context.SaveChangesAsync();

            return Ok(new { Message = "User deleted successfully" });
        }

        #region Role

        [HttpGet("roles")]
        public IActionResult GetRoles()
        {
            var roles = _roleManager.Roles.ToList();
            return Ok(roles);
        }

        [HttpPost("create")]
        public async Task<IActionResult> AddRole([FromBody] string role)
        {
            var appRole = new AppRole
            {
                Name = role
            };

            var result = await _roleManager.CreateAsync(appRole);
            if (result.Succeeded)
            {
                return CreatedAtAction(nameof(GetRoles), null); // HTTP 201 Created
            }
            else
            {
                return BadRequest(result.Errors); // HTTP 400 Bad Request
            }
        }

        [HttpPut("edit/{id}")]
        public async Task<IActionResult> EditRole([FromRoute] int id, [FromBody] string roleName)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                return NotFound($"Role with ID {id} not found");
            }

            role.Name = roleName;

            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded)
            {
                return Ok();
            }
            else
            {
                return BadRequest(result.Errors); 
            }
        }


        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                return NotFound("Role not found."); 
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                return Ok("Role removed."); 
            }
            else
            {
                return BadRequest(result.Errors); 
            }
        }

        #endregion
    }

}
