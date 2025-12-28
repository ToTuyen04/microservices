using eCommerce.Core.DTO;
using eCommerce.Core.ServiceContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace eCommerce.API.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUsersService _userService;
        public UsersController(IUsersService userService)
        {
            _userService = userService;
        }
        [HttpGet("{userID}")]
        public async Task<IActionResult> GetUserByUserID([FromRoute] Guid userID)
        {
            await Task.Delay(10000);
            //throw new NotImplementedException();

            if (userID == Guid.Empty)
            {
                return BadRequest("Invalid userID");
            }
            UserDTO? userDTO = await _userService.GetUserByUserID(userID);
            if(userDTO == null)
            {
                return NotFound(userDTO);
            }
            return Ok(userDTO);
        }
    }
}
