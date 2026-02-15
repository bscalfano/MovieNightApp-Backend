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
        public DbSet<FriendRequest> FriendRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure MovieNight relationship
            builder.Entity<MovieNight>()
                .HasOne(m => m.User)
                .WithMany(u => u.MovieNights)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure FriendRequest relationships
            builder.Entity<FriendRequest>()
                .HasOne(fr => fr.Sender)
                .WithMany(u => u.SentFriendRequests)
                .HasForeignKey(fr => fr.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<FriendRequest>()
                .HasOne(fr => fr.Receiver)
                .WithMany(u => u.ReceivedFriendRequests)
                .HasForeignKey(fr => fr.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prevent duplicate friend requests
            builder.Entity<FriendRequest>()
                .HasIndex(fr => new { fr.SenderId, fr.ReceiverId })
                .IsUnique();
        }
    }
}