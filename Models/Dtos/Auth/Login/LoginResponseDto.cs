namespace MangoStore_API.Models.Dtos.Auth
{
    // public class LoginResponseDto
    // {
    //     public int UserId { get; set; }
    //     public string UserName { get; set; }
    //     public string Token { get; set; }
    // }
    public class LoginResponseDto
    {
        public string Email { get; set; }
        public string Token { get; set; }
    }
}
