using ITBrainsBlogAPI.Models;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace ITBrainsBlogAPI.Models
{
    public class AppUser : IdentityUser<int>
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string ImageUrl { get; set; }
        [JsonIgnore]
        public List<Review> Reviews { get; set; }
        [JsonIgnore]
        public List<Blog> Blogs { get; set; }
        [JsonIgnore]
        public List<Like> Likes { get; set; }
        [JsonIgnore]
        public List<RefreshToken> RefreshTokens { get; set; }
        [JsonIgnore]
        public List<SavedBlog> SavedBlogs { get; set; }
    }
}