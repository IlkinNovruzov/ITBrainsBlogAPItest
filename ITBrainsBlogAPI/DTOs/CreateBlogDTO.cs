using System.ComponentModel.DataAnnotations;

namespace ITBrainsBlogAPI.DTOs
{
    public class CreateBlogDTO
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public List<string>? ImgURLs { get; set; }
    }
}
