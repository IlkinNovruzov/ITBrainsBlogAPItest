﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ITBrainsBlogAPI.Models
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, int>
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Like> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Review>()
       .HasOne(r => r.ParentReview)
       .WithMany(r => r.Reviews)
       .HasForeignKey(r => r.ParentReviewId)
       .OnDelete(DeleteBehavior.Restrict); // To avoid cycles or multiple cascade paths

            // Relationship with AppUser
            modelBuilder.Entity<Review>()
                .HasOne(r => r.AppUser)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship with Blog
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Blog)
                .WithMany(b => b.Reviews)
                .HasForeignKey(r => r.BlogId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Like>()
     .HasKey(l => new { l.AppUserId, l.BlogId });

            modelBuilder.Entity<Like>()
                .HasOne(l => l.AppUser)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.AppUserId)
                .OnDelete(DeleteBehavior.Restrict); // veya DeleteBehavior.NoAction

            modelBuilder.Entity<Like>()
                .HasOne(l => l.Blog)
                .WithMany(b => b.Likes)
                .HasForeignKey(l => l.BlogId)
                .OnDelete(DeleteBehavior.Restrict);
            base.OnModelCreating(modelBuilder);
        }
    }
}
