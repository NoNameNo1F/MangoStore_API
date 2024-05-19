using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Azure.Core;
using MangoStore_API.Data;
using MangoStore_API.Enums;
using MangoStore_API.Logging;
using MangoStore_API.Models;
using MangoStore_API.Models.Dtos.Auth;
using MangoStore_API.Repositories.IReposiitories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace MangoStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository userRepo;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private ApiResponse _response;
        private string secretKey;
        private ILogging logger;
        public AuthController(
            IUserRepository userRepo,
            IConfiguration config,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogging logger,
            ApiResponse apiResponse)
        {
            this.userRepo = userRepo;
            secretKey = config.GetValue<string>("AppSettings:Secret");
            _userManager = userManager;
            _roleManager = roleManager;
            this.logger = logger;
            _response = apiResponse;
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            User user = await userRepo.GetAsync(u => u.UserName.ToLower() == request.UserName.ToLower());

            bool isValid = await _userManager.CheckPasswordAsync(user, request.Password);

            if(!isValid)
            {
                _response.Result = new LoginRequestDto();
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Username or password is incorrect.");

                logger.Log("Username or password is incorrect.", "error");
                return BadRequest(_response);
            }

            // generate jwt token
            var role = await _userManager.GetRolesAsync(user);

            JwtSecurityTokenHandler tokenHandler = new();
            byte[] key = Encoding.ASCII.GetBytes(secretKey);

            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("fullname", user.Name),
                    new Claim("id", user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.UserName.ToString()),
                    new Claim(ClaimTypes.Role, role.FirstOrDefault()),
                }),

                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials
                (
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            LoginResponseDto loginResponseDto = new()
            {
                Email = user.Email,
                Token = tokenHandler.WriteToken(token)
            };

            if(loginResponseDto.Email == null || string.IsNullOrEmpty(loginResponseDto.Token))
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Username or password is incorrect.");

                logger.Log("Username or password is incorrect.", "error");
                return BadRequest(_response);
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = loginResponseDto;

            logger.Log($"User {request.UserName} Authenticate successfully.", "info");
            return Ok(_response);
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            User user = await userRepo.GetAsync(u => u.UserName.ToLower() == request.UserName.ToLower());

            if (user != null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Username already exists!");

                logger.Log($"Username already exists!", "error");
                return BadRequest(_response);
            }

            User newUser = new()
            {
                UserName = request.UserName,
                Email = request.UserName,
                NormalizedEmail = request.UserName.ToUpper(),
                Name = request.Name
            };

            try
            {
                var status = await _userManager.CreateAsync(newUser, request.Password);
                //.GetAwaiter().GetResult();
                if(status.Succeeded)
                {
                    // Enum.GetName(typeof(string), eRole.Admin)
                    //.GetAwaiter().GetResult() ==  await
                    if(! await _roleManager.RoleExistsAsync(eRole.Admin.ToString()))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(eRole.Admin.ToString()));
                        await _roleManager.CreateAsync(new IdentityRole(eRole.Customer.ToString()));
                    }
                    string role = Enum.GetName(typeof(eRole), request.Role).ToString();
                    if(role == eRole.Admin.ToString())
                    {
                        await _userManager.AddToRoleAsync(newUser, eRole.Admin.ToString());
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(newUser, eRole.Customer.ToString());
                    }

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;

                    logger.Log("User register successfully", "info");
                    return Ok(_response);
                }
            }
            catch (Exception exception)
            {
                //
            }
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.IsSuccess = false;
            _response.ErrorMessages.Add("Error occurs when registering");

            logger.Log("Error occurs when registering", "error");
            return BadRequest(_response);
        }

    }
}
