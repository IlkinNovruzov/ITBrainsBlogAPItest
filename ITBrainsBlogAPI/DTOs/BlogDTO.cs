using ITBrainsBlogAPI.Models;

namespace ITBrainsBlogAPI.DTOs
{
    public class BlogDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
        public string UserSurname { get; set; }
        public string UserImageUrl { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
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
        public int UserId { get; set; }
        public int ParentUserId { get; set; }
        public string Comment { get; set; }
        public string Date { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; } 
        public string UserSurname { get; set; } 
        public string UserImageUrl { get; set; }
        public string? ParentUserName { get; set; } 
        public string? ParentUserSurname { get; set; }
        public int ParentId { get; set; }
        public List<ReviewDTO> Replies { get; set; } 

    }
}
