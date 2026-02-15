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
    public class FollowController : ControllerBase
    {
        private readonly MovieNightAppContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FollowController(MovieNightAppContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }

        // GET: api/Follow/stats
        [HttpGet("stats")]
        public async Task<ActionResult<FollowStatsDto>> GetFollowStats()
        {
            var userId = GetUserId();

            var followersCount = await _context.UserFollows
                .Where(uf => uf.FollowingId == userId)
                .CountAsync();

            var followingCount = await _context.UserFollows
                .Where(uf => uf.FollowerId == userId)
                .CountAsync();

            return Ok(new FollowStatsDto
            {
                FollowersCount = followersCount,
                FollowingCount = followingCount
            });
        }

        // GET: api/Follow/search?query=john
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<UserSearchDto>>> SearchUsers([FromQuery] string query)
        {
            var currentUserId = GetUserId();

            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Query cannot be empty");
            }

            var users = await _userManager.Users
                .Where(u => u.Id != currentUserId &&
                    (u.Email!.Contains(query) ||
                     (u.FirstName != null && u.FirstName.Contains(query)) ||
                     (u.LastName != null && u.LastName.Contains(query))))
                .Take(10)
                .ToListAsync();

            var userIds = users.Select(u => u.Id).ToList();
            var followingIds = await _context.UserFollows
                .Where(uf => uf.FollowerId == currentUserId && userIds.Contains(uf.FollowingId))
                .Select(uf => uf.FollowingId)
                .ToListAsync();

            var result = users.Select(u => new UserSearchDto
            {
                Id = u.Id,
                Email = u.Email!,
                FirstName = u.FirstName,
                LastName = u.LastName,
                ProfilePictureUrl = u.ProfilePictureUrl,
                IsFollowing = followingIds.Contains(u.Id)
            });

            return Ok(result);
        }

        // GET: api/Follow/followers
        [HttpGet("followers")]
        public async Task<ActionResult<IEnumerable<UserSearchDto>>> GetFollowers()
        {
            var userId = GetUserId();

            var followers = await _context.UserFollows
                .Where(uf => uf.FollowingId == userId)
                .Include(uf => uf.Follower)
                .Select(uf => new UserSearchDto
                {
                    Id = uf.Follower.Id,
                    Email = uf.Follower.Email!,
                    FirstName = uf.Follower.FirstName,
                    LastName = uf.Follower.LastName,
                    ProfilePictureUrl = uf.Follower.ProfilePictureUrl,
                    IsFollowing = _context.UserFollows.Any(f => f.FollowerId == userId && f.FollowingId == uf.Follower.Id)
                })
                .ToListAsync();

            return Ok(followers);
        }

        // GET: api/Follow/following
        [HttpGet("following")]
        public async Task<ActionResult<IEnumerable<UserSearchDto>>> GetFollowing()
        {
            var userId = GetUserId();

            var following = await _context.UserFollows
                .Where(uf => uf.FollowerId == userId)
                .Include(uf => uf.Following)
                .Select(uf => new UserSearchDto
                {
                    Id = uf.Following.Id,
                    Email = uf.Following.Email!,
                    FirstName = uf.Following.FirstName,
                    LastName = uf.Following.LastName,
                    ProfilePictureUrl = uf.Following.ProfilePictureUrl,
                    IsFollowing = true
                })
                .ToListAsync();

            return Ok(following);
        }

        // POST: api/Follow/{userId}
        [HttpPost("{userId}")]
        public async Task<IActionResult> FollowUser(string userId)
        {
            var currentUserId = GetUserId();

            if (currentUserId == userId)
            {
                return BadRequest("Cannot follow yourself");
            }

            var userToFollow = await _userManager.FindByIdAsync(userId);
            if (userToFollow == null)
            {
                return NotFound("User not found");
            }

            var existingFollow = await _context.UserFollows
                .FirstOrDefaultAsync(uf => uf.FollowerId == currentUserId && uf.FollowingId == userId);

            if (existingFollow != null)
            {
                return BadRequest("Already following this user");
            }

            var follow = new UserFollow
            {
                FollowerId = currentUserId,
                FollowingId = userId
            };

            _context.UserFollows.Add(follow);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/Follow/{userId}
        [HttpDelete("{userId}")]
        public async Task<IActionResult> UnfollowUser(string userId)
        {
            var currentUserId = GetUserId();

            var follow = await _context.UserFollows
                .FirstOrDefaultAsync(uf => uf.FollowerId == currentUserId && uf.FollowingId == userId);

            if (follow == null)
            {
                return NotFound("Not following this user");
            }

            _context.UserFollows.Remove(follow);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}