namespace ITBrainsBlogAPI.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public int AppUserId { get; set; }
        public AppUser AppUser { get; set; }
        public bool IsActive { get; set; }
    }

}
