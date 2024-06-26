using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ITBrainsBlogAPI.Models;
using ITBrainsBlogAPI.DTOs;
using ITBrainsBlogAPI.Services;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs.Models;
using System.Net.Mail;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using JwtClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;
using Microsoft.AspNetCore.WebUtilities;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Google.Apis.Auth.OAuth2.Requests;
using Microsoft.Extensions.Primitives;

namespace ITBrainsBlogAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly TokenService _tokenService;
        public AzureBlobService _service;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailService emailService, IConfiguration configuration, TokenService tokenService, AzureBlobService service)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _configuration = configuration;
            _tokenService = tokenService;
            _service = service;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new AppUser { UserName = model.Email, Email = model.Email, Name = model.Name, Surname = model.Surname, ImageUrl = "https://itbblogstorage.blob.core.windows.net/itbcontainer/0e39b3d3-971a-43c4-bf37-5f517b6bd0c8_defaultimage.png" };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                var confirmationLink = $"http://localhost:5173/confirm-email?userId={user.Id}&token={encodedToken}";
                // var confirmationLink = Url.Action(,"ConfirmEmail", "Account", new { userId = user.Id, token = token }, Request.Scheme);
                //  var confirmationLink = $"http://localhost:5173/confirm-email?userId={user.Id}&token={token}";
                try
                {
                    await _emailService.SendEmailAsync(model.Email, "Confirm your email", $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");
                }
                catch (SmtpException ex)
                {
                    return StatusCode(500, $"Error sending confirmation email: {ex.Message}");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Error sending confirmation email: {ex.Message}");
                }
                await _signInManager.SignInAsync(user, isPersistent: false);
                return Ok("Register is successfully");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return NotFound("User not found");
            }
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                var confirmationLink = $"http://localhost:5173/confirm-email?userId={user.Id}&token={encodedToken}";
                try
                {
                    await _emailService.SendEmailAsync(model.Email, "Confirm your email", $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");
                }
                catch (Exception ex)
                {
                    // Log the exception
                    return StatusCode(500, "Error sending confirmation email." + ex.Message);
                }

                return BadRequest("Email not confirmed. Confirmation email has been sent.");
            }
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                //var ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();
                var (jwtToken, refreshToken) = _tokenService.GenerateTokens(user);
                return Ok(new { JwtToken = jwtToken, RefreshToken = refreshToken });
                //var jwtToken = GenerateJwtToken(user);
                //return Ok(jwtToken);
            }

            if (result.IsLockedOut)
            {
                return BadRequest("User account locked out.");
            }

            return BadRequest("Invalid login attempt.");
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(int userId, string token)
        {
            if (userId == 0 || token == null)
            {
                return BadRequest("Invalid email confirmation request.");
            }

            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return BadRequest("Invalid email confirmation request.");
            }
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            if (result.Succeeded)
            {
                return Ok("Email confirmed successfully.");
            }

            return BadRequest("Error confirming your email.");
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok("Logout");
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                user.UserName,
                user.Email
            });
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
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound(new { Message = "User Not Found" });
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { Message = "User deleted successfully" });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser([FromRoute] int id)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound(new { Message = "User Not Found" });
            }
            return Ok(user);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email is required.");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (!(await _userManager.IsEmailConfirmedAsync(user)))
            {
                return BadRequest("Email is not confirmed. Please confirm your email before requesting a password reset.");
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var confirmationLink = $"http://localhost:5173/reset-password?userId={user.Id}&token={encodedToken}";

            try
            {
                await _emailService.SendEmailAsync(email, "Reset Password", $"Please reset your password by <a href='{confirmationLink}'>clicking here</a>.");
            }
            catch (SmtpException ex)
            {
                return StatusCode(500, $"Error sending reset email: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error sending reset email: {ex.Message}");
            }

            return Ok("If the email is registered, a password reset link will be sent to the email address.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(model.UserId.ToString());
            if (user == null)
            {
                return BadRequest("Invalid email address.");
            }

            if (model.Password != model.ConfirmPassword)
            {
                return BadRequest("The password and confirmation password do not match.");
            }
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.Password);
            if (result.Succeeded)
            {
                return Ok("Your password has been reset successfully.");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);
        }

        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin([FromBody] string token)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(token);
                return Ok(new { message = "Google authentication successful", user = payload });
            }
            catch (InvalidJwtException)
            {
                return BadRequest("Invalid token.");
            }
        }


        [HttpPost("{id}/upload-profile-image")]
        public async Task<IActionResult> UploadProfileImage([FromRoute] int id, IFormFile file)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }
            if (!FileExtensions.IsImage(file))
            {
                return BadRequest("This file type is not accepted.");
            }

            var fileName = await _service.UploadFile(file);
            var profileImageUrl = $"https://itbblogstorage.blob.core.windows.net/itbcontainer/{fileName}";
            user.ImageUrl = profileImageUrl;
            await _userManager.UpdateAsync(user);
            return Ok(new { Message = "Profile image uploaded successfully", ImageUrl = user.ImageUrl });
        }

        [HttpPost("{id}/remove-profile-image")]
        public async Task<IActionResult> RemoveProfileImage([FromRoute] int id)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }
            user.ImageUrl = "https://itbblogstorage.blob.core.windows.net/itbcontainer/0e39b3d3-971a-43c4-bf37-5f517b6bd0c8_defaultimage.png";
            await _userManager.UpdateAsync(user);
            return Ok(new { Message = "Profile image removed successfully", ImageUrl = user.ImageUrl });
        }

        [HttpPut("edit")]
        public async Task<IActionResult> EditUser([FromForm] UpdateUserDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Id == model.UserId);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }
            if (model.ImageDeleted)
            {
                user.ImageUrl = "https://itbblogstorage.blob.core.windows.net/itbcontainer/0e39b3d3-971a-43c4-bf37-5f517b6bd0c8_defaultimage.png";
            }
            if (model.ImgFile != null)
            {
                if (!FileExtensions.IsImage(model.ImgFile))
                {
                    return BadRequest("This file type is not accepted.");
                }

                var fileName = await _service.UploadFile(model.ImgFile);
                var profileImageUrl = $"https://itbblogstorage.blob.core.windows.net/itbcontainer/{fileName}";
                user.ImageUrl = profileImageUrl;
            }
            user.Name = model.Name;
            user.Surname = model.Surname;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Ok("User updated");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);

        }




        [HttpPost("refresh-token")]
        public IActionResult RefreshToken([FromBody] string refreshToken)
        {
            try
            {
                var newJwtToken = _tokenService.RefreshJwtToken(refreshToken);
                return Ok(new { jwtToken = newJwtToken });
            }
            catch (SecurityTokenException)
            {
                return Unauthorized("Invalid refresh token");
            }
        }







        private async Task<GoogleJsonWebSignature.Payload> ValidateGoogleTokenAsync(string token)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { _configuration["Authentication:Google:ClientId"] }
                };
                var payload = await GoogleJsonWebSignature.ValidateAsync(token, settings);
                return payload;
            }
            catch
            {
                return null;
            }
        }
        private string GenerateJwtToken(AppUser user)
        {
            var claims = new[]
            {
                  new Claim(JwtClaimNames.Sub, user.UserName),
                  new Claim(JwtClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private string GenerateJwtToken(GoogleJsonWebSignature.Payload payload)
        {
            var claims = new[]
            {
            new Claim(JwtClaimNames.Sub, payload.Subject),
            new Claim(JwtClaimNames.Email, payload.Email),
            new Claim(JwtClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Authentication:Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Authentication:Jwt:Issuer"],
                audience: _configuration["Authentication:Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
    //userin bloglari
}
