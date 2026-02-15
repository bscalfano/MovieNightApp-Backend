using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieNightApp.Data;
using MovieNightApp.Models;
using MovieNightApp.DTOs;
using System.Security.Claims;

namespace MovieNightApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MovieNightsController : ControllerBase
    {
        private readonly MovieNightAppContext _context;

        public MovieNightsController(MovieNightAppContext context)
        {
            _context = context;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }

        // GET: api/MovieNights
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovieNight>>> GetMovieNights()
        {
            var userId = GetUserId();
            return await _context.MovieNights
                .Where(m => m.UserId == userId)
                .OrderBy(m => m.ScheduledDate)
                .ThenBy(m => m.StartTime)
                .ToListAsync();
        }

        // GET: api/MovieNights/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetMovieNight(int id)
        {
            var userId = GetUserId();
            var movieNight = await _context.MovieNights
                .Where(m => m.UserId == userId && m.Id == id)
                .FirstOrDefaultAsync();

            if (movieNight == null)
            {
                return NotFound();
            }

            // Get attendees
            var attendees = await _context.MovieNightAttendees
                .AsNoTracking()
                .Where(a => a.MovieNightId == id)
                .Include(a => a.User)
                .Select(a => new
                {
                    userId = a.UserId,
                    email = a.User.Email,
                    firstName = a.User.FirstName,
                    lastName = a.User.LastName,
                    profilePictureUrl = a.User.ProfilePictureUrl,
                    rsvpedAt = a.RsvpedAt
                })
                .ToListAsync();

            return Ok(new
            {
                movieNight = movieNight,
                attendees = attendees
            });
        }

        // GET: api/MovieNights/upcoming
        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<MovieNight>>> GetUpcomingMovieNights()
        {
            var userId = GetUserId();
            var today = DateTime.Today;
            return await _context.MovieNights
                .Where(m => m.UserId == userId && m.ScheduledDate >= today)
                .OrderBy(m => m.ScheduledDate)
                .ThenBy(m => m.StartTime)
                .ToListAsync();
        }

        // GET: api/MovieNights/past
        [HttpGet("past")]
        public async Task<ActionResult<IEnumerable<MovieNight>>> GetPastMovieNights()
        {
            var userId = GetUserId();
            var today = DateTime.Today;
            return await _context.MovieNights
                .Where(m => m.UserId == userId && m.ScheduledDate < today)
                .OrderByDescending(m => m.ScheduledDate)
                .ThenByDescending(m => m.StartTime)
                .ToListAsync();
        }

        // POST: api/MovieNights
        [HttpPost]
        public async Task<ActionResult<MovieNight>> PostMovieNight([FromBody] CreateMovieNightDto dto)
        {
            var userId = GetUserId();

            var movieNight = new MovieNight
            {
                MovieTitle = dto.MovieTitle,
                ScheduledDate = dto.ScheduledDate,
                StartTime = dto.StartTime,
                Notes = dto.Notes,
                ImageUrl = dto.ImageUrl,
                Genre = dto.Genre,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.MovieNights.Add(movieNight);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMovieNight), new { id = movieNight.Id }, movieNight);
        }

        // PUT: api/MovieNights/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMovieNight(int id, [FromBody] UpdateMovieNightDto dto)
        {
            var userId = GetUserId();
            var movieNight = await _context.MovieNights
                .Where(m => m.UserId == userId && m.Id == id)
                .FirstOrDefaultAsync();

            if (movieNight == null)
            {
                return NotFound();
            }

            movieNight.MovieTitle = dto.MovieTitle;
            movieNight.ScheduledDate = dto.ScheduledDate;
            movieNight.StartTime = dto.StartTime;
            movieNight.Notes = dto.Notes;
            movieNight.ImageUrl = dto.ImageUrl;
            movieNight.Genre = dto.Genre;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovieNightExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/MovieNights/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovieNight(int id)
        {
            var userId = GetUserId();
            var movieNight = await _context.MovieNights
                .Where(m => m.UserId == userId && m.Id == id)
                .FirstOrDefaultAsync();

            if (movieNight == null)
            {
                return NotFound();
            }

            _context.MovieNights.Remove(movieNight);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MovieNightExists(int id)
        {
            var userId = GetUserId();
            return _context.MovieNights.Any(e => e.Id == id && e.UserId == userId);
        }
    }
}