using eCommerce.Core.DTO;
using eCommerce.Core.ServiceContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUsersService _userService;
        public AuthController(IUsersService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request) 
        {
            if (request == null)
            {
                return BadRequest("Invalid login data");
            }
            AuthenticationResponse? authResponse = await _userService.Login(request);
            if(authResponse == null || authResponse.Success == false)
            {
                return Unauthorized("Invalid email or password");
            }
            return Ok(authResponse);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request) 
        { 
            AuthenticationResponse? authResponse = await _userService.Register(request);
            if(authResponse == null || authResponse.Success == false)
            {
                return BadRequest("User registration failed");
            }
            return Ok(authResponse);
        }
    }
}
