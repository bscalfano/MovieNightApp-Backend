using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieNightApp.Data;
using MovieNightApp.Models;
using MovieNightApp.DTOs;

namespace MovieNightApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieNightsController : ControllerBase
    {
        private readonly MovieNightAppContext _context;

        public MovieNightsController(MovieNightAppContext context)
        {
            _context = context;
        }

        // GET: api/MovieNights
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovieNight>>> GetMovieNights()
        {
            return await _context.MovieNights
                .OrderBy(m => m.ScheduledDate)
                .ThenBy(m => m.StartTime)
                .ToListAsync();
        }

        // GET: api/MovieNights/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MovieNight>> GetMovieNight(int id)
        {
            var movieNight = await _context.MovieNights.FindAsync(id);

            if (movieNight == null)
            {
                return NotFound();
            }

            return movieNight;
        }

        // GET: api/MovieNights/upcoming
        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<MovieNight>>> GetUpcomingMovieNights()
        {
            var today = DateTime.Today;
            return await _context.MovieNights
                .Where(m => m.ScheduledDate >= today)
                .OrderBy(m => m.ScheduledDate)
                .ThenBy(m => m.StartTime)
                .ToListAsync();
        }

        // GET: api/MovieNights/past
        [HttpGet("past")]
        public async Task<ActionResult<IEnumerable<MovieNight>>> GetPastMovieNights()
        {
            var today = DateTime.Today;
            return await _context.MovieNights
                .Where(m => m.ScheduledDate < today)
                .OrderByDescending(m => m.ScheduledDate)
                .ThenByDescending(m => m.StartTime)
                .ToListAsync();
        }

        // POST: api/MovieNights
        [HttpPost]
        public async Task<ActionResult<MovieNight>> PostMovieNight([FromBody] CreateMovieNightDto dto)
        {
            var movieNight = new MovieNight
            {
                MovieTitle = dto.MovieTitle,
                ScheduledDate = dto.ScheduledDate,
                StartTime = dto.StartTime,
                Notes = dto.Notes,
                ImageUrl = dto.ImageUrl,
                Genre = dto.Genre,
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
            var movieNight = await _context.MovieNights.FindAsync(id);

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
            var movieNight = await _context.MovieNights.FindAsync(id);
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
            return _context.MovieNights.Any(e => e.Id == id);
        }
    }
}