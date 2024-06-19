using ITBrainsBlogAPI.Models;

namespace ITBrainsBlogAPI.DTOs
{
    public class ReviewDTO
    {
        public string Comment { get; set; }
        public int AppUserId { get; set; }
        public int BlogId { get; set; }
        public int? ParentReviewId { get; set; }
    }
}
 