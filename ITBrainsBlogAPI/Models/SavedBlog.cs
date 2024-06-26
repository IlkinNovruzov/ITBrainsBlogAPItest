using System.ComponentModel.DataAnnotations;

namespace ITBrainsBlogAPI.Models
{
    public class SavedBlog
    {
        public DateTime SavedAt { get; set; }

        public int AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        public int BlogId { get; set; }
        public Blog Blog { get; set; }
    }
}
