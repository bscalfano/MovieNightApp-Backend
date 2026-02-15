using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieNightApp.Data;
using MovieNightApp.DTOs;
using MovieNightApp.Models;
using System.Security.Claims;

namespace MovieNightApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MovieNightAppContext _context;

        public ProfileController(UserManager<ApplicationUser> userManager, MovieNightAppContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }

        // GET: api/Profile
        [HttpGet]
        public async Task<ActionResult<UserProfileDto>> GetProfile()
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var totalMovieNights = await _context.MovieNights
                .Where(m => m.UserId == userId)
                .CountAsync();

            var upcomingMovieNights = await _context.MovieNights
                .Where(m => m.UserId == userId && m.ScheduledDate >= DateTime.Today)
                .CountAsync();

            var followersCount = await _context.UserFollows
                .Where(uf => uf.FollowingId == userId)
                .CountAsync();

            var followingCount = await _context.UserFollows
                .Where(uf => uf.FollowerId == userId)
                .CountAsync();

            return Ok(new UserProfileDto
            {
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfilePictureUrl = user.ProfilePictureUrl,
                CreatedAt = user.CreatedAt,
                TotalMovieNights = totalMovieNights,
                UpcomingMovieNights = upcomingMovieNights,
                FollowersCount = followersCount,
                FollowingCount = followingCount
            });
        }

        // PUT: api/Profile
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Check if email is already taken by another user
            if (dto.Email != user.Email)
            {
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null && existingUser.Id != userId)
                {
                    return BadRequest(new { message = "Email is already taken" });
                }
            }

            user.Email = dto.Email;
            user.UserName = dto.Email;
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.ProfilePictureUrl = dto.ProfilePictureUrl;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }

        // POST: api/Profile/change-password
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { message = "Password changed successfully" });
        }

        // DELETE: api/Profile
        [HttpDelete]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Delete all movie nights (cascade delete should handle this, but being explicit)
            var movieNights = await _context.MovieNights
                .Where(m => m.UserId == userId)
                .ToListAsync();
            _context.MovieNights.RemoveRange(movieNights);

            // Delete user
            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }
    }
}