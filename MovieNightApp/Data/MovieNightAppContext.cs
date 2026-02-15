using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MovieNightApp.Models;

namespace MovieNightApp.Data
{
    public class MovieNightAppContext : IdentityDbContext<ApplicationUser>
    {
        public MovieNightAppContext(DbContextOptions<MovieNightAppContext> options)
            : base(options)
        {
        }

        public DbSet<MovieNight> MovieNights { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationship
            builder.Entity<MovieNight>()
                .HasOne(m => m.User)
                .WithMany(u => u.MovieNights)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}