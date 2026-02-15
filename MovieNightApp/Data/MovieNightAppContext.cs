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
        public DbSet<UserFollow> UserFollows { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure MovieNight relationship
            builder.Entity<MovieNight>()
                .HasOne(m => m.User)
                .WithMany(u => u.MovieNights)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure UserFollow relationships
            builder.Entity<UserFollow>()
                .HasOne(uf => uf.Follower)
                .WithMany(u => u.Following)
                .HasForeignKey(uf => uf.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserFollow>()
                .HasOne(uf => uf.Following)
                .WithMany(u => u.Followers)
                .HasForeignKey(uf => uf.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prevent duplicate follows
            builder.Entity<UserFollow>()
                .HasIndex(uf => new { uf.FollowerId, uf.FollowingId })
                .IsUnique();
        }
    }
}