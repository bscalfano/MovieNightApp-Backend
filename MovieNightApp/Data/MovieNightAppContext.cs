using Microsoft.EntityFrameworkCore;
using MovieNightApp.Models;

namespace MovieNightApp.Data
{
    public class MovieNightAppContext : DbContext
    {
        public MovieNightAppContext(DbContextOptions<MovieNightAppContext> options)
            : base(options)
        {
        }

        public DbSet<MovieNight> MovieNights { get; set; }
    }
}
