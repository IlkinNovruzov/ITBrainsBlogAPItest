using System.ComponentModel.DataAnnotations;

namespace ITBrainsBlogAPI.DTOs
{
    public class BlogDTO
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public List<IFormFile>? ImgFiles { get; set; }
    }
}
