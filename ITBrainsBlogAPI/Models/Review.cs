using System.Text.Json.Serialization;

namespace ITBrainsBlogAPI.Models
{
    public class Review
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public DateTime Date { get; set; }
        public int AppUserId { get; set; }
        public AppUser AppUser { get; set; }
        public int BlogId { get; set; }
        [JsonIgnore]
        public Blog Blog { get; set; }
        public int? ParentReviewId { get; set; }
        [JsonIgnore]
        public Review? ParentReview { get; set; }
        public List<Review> Reviews { get; set; }
        public int LikeCount { get; set; }
        public bool IsActive { get; set; }
    }
}
