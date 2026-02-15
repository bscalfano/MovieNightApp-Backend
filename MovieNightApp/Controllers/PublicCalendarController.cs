using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieNightApp.Data;
using MovieNightApp.Models;
using System.Security.Claims;

namespace MovieNightApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PublicCalendarController : ControllerBase
    {
        private readonly MovieNightAppContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PublicCalendarController(MovieNightAppContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }

        // GET: api/PublicCalendar/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserCalendar(string userId)
        {
            try
            {
                var currentUserId = GetUserId();

                // Check if they are friends (using enum value 1 for Accepted)
                var areFriends = await _context.FriendRequests
                    .AnyAsync(fr => fr.Status == (FriendRequestStatus)1 &&
                        ((fr.SenderId == currentUserId && fr.ReceiverId == userId) ||
                         (fr.SenderId == userId && fr.ReceiverId == currentUserId)));

                if (!areFriends && currentUserId != userId)
                {
                    return StatusCode(403, new { message = "You must be friends to view this calendar" });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Use AsNoTracking and select only the fields we need to avoid circular reference
                var movieNights = await _context.MovieNights
                    .AsNoTracking()
                    .Where(m => m.UserId == userId && m.ScheduledDate >= DateTime.Today)
                    .OrderBy(m => m.ScheduledDate)
                    .ThenBy(m => m.StartTime)
                    .Select(m => new
                    {
                        m.Id,
                        m.MovieTitle,
                        m.ScheduledDate,
                        m.StartTime,
                        m.Notes,
                        m.ImageUrl,
                        m.Genre,
                        m.CreatedAt
                    })
                    .ToListAsync();

                var totalMovieNights = await _context.MovieNights
                    .Where(m => m.UserId == userId)
                    .CountAsync();

                var response = new
                {
                    user = new
                    {
                        id = user.Id,
                        email = user.Email,
                        firstName = user.FirstName,
                        lastName = user.LastName,
                        profilePictureUrl = user.ProfilePictureUrl
                    },
                    movieNights = movieNights,
                    totalMovieNights = totalMovieNights,
                    isOwnCalendar = currentUserId == userId
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log the actual error
                Console.WriteLine($"Error in GetUserCalendar: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }
    }
}