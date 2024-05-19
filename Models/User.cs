using Microsoft.AspNetCore.Identity;

namespace MangoStore_API.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; }
    }
}
