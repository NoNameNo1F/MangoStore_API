using MangoStore_API.Enums;

namespace MangoStore_API.Models.Dtos.Auth
{
    public class RegisterRequestDto
    {
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public eRole Role {get; set;}
    }
}
