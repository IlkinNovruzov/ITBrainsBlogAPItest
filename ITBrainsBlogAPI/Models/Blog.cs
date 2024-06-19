namespace ITBrainsBlogAPI.Models
{
    public class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public int AppUserId { get; set; }
        public AppUser AppUser { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<Like> Likes { get; set; }
        public List<Image> Images { get; set; }
        public List<Review> Reviews { get; set; }
        public int ViewCount { get; set; } 
        public int ReviewCount => Reviews?.Count ?? 0;
        public int LikeCount => Likes?.Count ?? 0;
    }
}
