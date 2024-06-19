namespace ITBrainsBlogAPI.DTOs
{
    public class UpdateUserDTO
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public IFormFile ImgFile { get; set; }
    }
}
