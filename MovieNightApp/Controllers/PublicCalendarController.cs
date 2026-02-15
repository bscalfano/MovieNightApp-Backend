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

                // Get friends count
                var friendsCount = await _context.FriendRequests
                    .Where(fr => fr.Status == (FriendRequestStatus)1 &&
                        (fr.SenderId == userId || fr.ReceiverId == userId))
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
                    friendsCount = friendsCount,
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

        // GET: api/PublicCalendar/movie/{movieNightId}
        [HttpGet("movie/{movieNightId}")]
        public async Task<IActionResult> GetMovieNightDetails(int movieNightId)
        {
            var currentUserId = GetUserId();

            var movieNight = await _context.MovieNights
                .AsNoTracking()
                .Where(m => m.Id == movieNightId)
                .Select(m => new
                {
                    m.Id,
                    m.MovieTitle,
                    m.ScheduledDate,
                    m.StartTime,
                    m.Notes,
                    m.ImageUrl,
                    m.Genre,
                    m.CreatedAt,
                    m.UserId
                })
                .FirstOrDefaultAsync();

            if (movieNight == null)
            {
                return NotFound(new { message = "Movie night not found" });
            }

            // Check if they are friends with the owner or if they are the owner
            if (movieNight.UserId != currentUserId)
            {
                var areFriends = await _context.FriendRequests
                    .AnyAsync(fr => fr.Status == (FriendRequestStatus)1 &&
                        ((fr.SenderId == currentUserId && fr.ReceiverId == movieNight.UserId) ||
                         (fr.SenderId == movieNight.UserId && fr.ReceiverId == currentUserId)));

                if (!areFriends)
                {
                    return StatusCode(403, new { message = "You must be friends to view this movie night" });
                }
            }

            // Get attendees
            var attendees = await _context.MovieNightAttendees
                .AsNoTracking()
                .Where(a => a.MovieNightId == movieNightId)
                .Include(a => a.User)
                .Select(a => new AttendeeDto
                {
                    UserId = a.UserId,
                    Email = a.User.Email!,
                    FirstName = a.User.FirstName,
                    LastName = a.User.LastName,
                    ProfilePictureUrl = a.User.ProfilePictureUrl,
                    RsvpedAt = a.RsvpedAt
                })
                .ToListAsync();

            // Check if current user is attending
            var isAttending = attendees.Any(a => a.UserId == currentUserId);

            return Ok(new
            {
                movieNight = movieNight,
                attendees = attendees,
                isAttending = isAttending,
                isOwner = movieNight.UserId == currentUserId
            });
        }

        // POST: api/PublicCalendar/movie/{movieNightId}/attend
        [HttpPost("movie/{movieNightId}/attend")]
        public async Task<IActionResult> AttendMovieNight(int movieNightId)
        {
            var currentUserId = GetUserId();

            var movieNight = await _context.MovieNights
                .FirstOrDefaultAsync(m => m.Id == movieNightId);

            if (movieNight == null)
            {
                return NotFound(new { message = "Movie night not found" });
            }

            // Check if they are friends with the owner
            var areFriends = await _context.FriendRequests
                .AnyAsync(fr => fr.Status == (FriendRequestStatus)1 &&
                    ((fr.SenderId == currentUserId && fr.ReceiverId == movieNight.UserId) ||
                     (fr.SenderId == movieNight.UserId && fr.ReceiverId == currentUserId)));

            // Allow if friends or if it's their own movie night
            if (!areFriends && movieNight.UserId != currentUserId)
            {
                return StatusCode(403, new { message = "You must be friends to attend this movie night" });
            }

            // Check if already attending
            var existingAttendee = await _context.MovieNightAttendees
                .FirstOrDefaultAsync(a => a.MovieNightId == movieNightId && a.UserId == currentUserId);

            if (existingAttendee != null)
            {
                return BadRequest(new { message = "You are already attending this movie night" });
            }

            var attendee = new MovieNightAttendee
            {
                MovieNightId = movieNightId,
                UserId = currentUserId
            };

            _context.MovieNightAttendees.Add(attendee);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Successfully RSVP'd to movie night" });
        }

        // DELETE: api/PublicCalendar/movie/{movieNightId}/attend
        [HttpDelete("movie/{movieNightId}/attend")]
        public async Task<IActionResult> UnattendMovieNight(int movieNightId)
        {
            var currentUserId = GetUserId();

            var attendee = await _context.MovieNightAttendees
                .FirstOrDefaultAsync(a => a.MovieNightId == movieNightId && a.UserId == currentUserId);

            if (attendee == null)
            {
                return NotFound(new { message = "You are not attending this movie night" });
            }

            _context.MovieNightAttendees.Remove(attendee);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Successfully removed RSVP" });
        }
    }
}