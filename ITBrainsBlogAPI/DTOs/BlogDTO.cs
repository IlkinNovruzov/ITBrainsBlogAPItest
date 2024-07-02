using ITBrainsBlogAPI.Models;

namespace ITBrainsBlogAPI.DTOs
{
    public class BlogDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
        public string UserSurname { get; set; }
        public string UserImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<ImageDTO> Images { get; set; }
        public List<ReviewDTO> Reviews { get; set; }
        public int ViewCount { get; set; }
        public int ReviewCount { get; set; }
        public int LikeCount { get; set; }
        public int SaveCount { get; set; }
    }

    public class ImageDTO
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
    }
    public class ReviewDTO
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public DateTime? Date { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; } 
        public string UserSurname { get; set; }
        public string UserImageUrl { get; set; }
        public int ParentId { get; set; }
        public List<ReviewDTO> Replies { get; set; } 

    }
}
