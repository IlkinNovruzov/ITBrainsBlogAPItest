namespace ITBrainsBlogAPI.DTOs
{
    public class ResetPasswordDTO
    {
        public int UserId { get; set; }
        public string Token { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
