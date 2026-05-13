using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStaff.BusinessLayers.Interfaces;
using NovaStaff.Models.Enums;

namespace NovaStaff.API.Controllers
{
    [ApiController]
    [Route("api/dev")]
    public class DevController : ControllerBase
    {
        private readonly ITokenService _tokenService;

        public DevController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpGet("token")]
        public IActionResult GetToken()
        {
            var fakeUser = new User
            {
                UserID = 1,
                Username = "test",
                Role = UserRole.Admin
            };

            var token = _tokenService.GenerateAccessToken(fakeUser);

            return Ok(token);
        }

        [Authorize]
        [HttpGet("private")]
        public IActionResult Private()
        {
            return Ok("Authorized");
        }
    }
}
