using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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
        public DbSet<SavedBlog> SavedBlogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }

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

            modelBuilder.Entity<Notification>()
              .HasOne(n => n.AppUser)
              .WithMany(u => u.Notifications)
              .HasForeignKey(n => n.AppUserId)
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
                .OnDelete(DeleteBehavior.Restrict); // or DeleteBehavior.NoAction

            modelBuilder.Entity<Like>()
                .HasOne(l => l.Blog)
                .WithMany(b => b.Likes)
                .HasForeignKey(l => l.BlogId)
                .OnDelete(DeleteBehavior.Restrict);

          
            modelBuilder.Entity<SavedBlog>()
   .HasKey(l => new { l.AppUserId, l.BlogId });

            modelBuilder.Entity<SavedBlog>()
                .HasOne(l => l.AppUser)
                .WithMany(u => u.SavedBlogs)
                .HasForeignKey(l => l.AppUserId)
                .OnDelete(DeleteBehavior.Restrict); // or DeleteBehavior.NoAction

            modelBuilder.Entity<SavedBlog>()
                .HasOne(l => l.Blog)
                .WithMany(b => b.SavedBlogs)
                .HasForeignKey(l => l.BlogId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AppRole>().HasData(
               new AppRole { Id = 1, Name = "Admin", NormalizedName = "ADMIN" },
                 new AppRole { Id = 2, Name = "User", NormalizedName = "USER" }
             );
            var hasher = new PasswordHasher<AppUser>();
            modelBuilder.Entity<AppUser>().HasData(
              new AppUser
              {
                  Id = 1,
                  UserName = "ilkin.admin",
                  Name = "Ilkin",
                  Surname = "Novruzov",
                  ImageUrl="Image",
                  PasswordHash = hasher.HashPassword(null, "Admin.1234"),
                  Email = "inovruzov2004@gmail.com",
                  EmailConfirmed = true,
                  NormalizedUserName = "ILKIN.ADMIN",
                  NormalizedEmail = "INOVRUZOV2004@GMAIL.COM",
                  LockoutEnabled = true,
                  SecurityStamp = Guid.NewGuid().ToString()
              }
              );

            modelBuilder.Entity<IdentityUserRole<int>>().HasData(
       new IdentityUserRole<int> { UserId = 1, RoleId = 1 });


            // modelBuilder.Entity<Blog>().Navigation(b => b.Images).AutoInclude();

            //   modelBuilder.Entity<Blog>().Navigation(b => b.Reviews);
            //        modelBuilder.Entity<Blog>()
            //.Navigation(b => b.Reviews)
            //.AutoInclude();

            //        modelBuilder.Entity<Blog>().Navigation(b => b.AppUser).AutoInclude();

            //        modelBuilder.Entity<Blog>().Navigation(b => b.Likes).AutoInclude();

            //        modelBuilder.Entity<Blog>().Navigation(b => b.SavedBlogs).AutoInclude();

            //modelBuilder.Entity<Review>().Navigation(r => r.AppUser).AutoInclude();

            base.OnModelCreating(modelBuilder);
        }
    }
}
