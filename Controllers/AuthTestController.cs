using MangoStore_API.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MangoStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AuthTestController : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<string>> GetSomeThing()
        {
            return "You are authenticated";
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<string>> GetSomeThing(int someInt)
        {
            return "You are Authorized with Admin";
        }
    }
}
