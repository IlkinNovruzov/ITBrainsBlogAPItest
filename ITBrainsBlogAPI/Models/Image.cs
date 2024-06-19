using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ITBrainsBlogAPI.Models
{
    public class Image
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        
        public int BlogId { get; set; }
        [JsonIgnore]
        public Blog Blog { get; set; }

        public bool IsActive { get; set; }

        [NotMapped]
        public IFormFile ImageFile { get; set; }
    }
}
