using Entities.DataTransferObjects.User;
using Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace StripeIntegration.Controllers
{
    [Route("api/[controller]")]
    public class AuthenticateController : BaseController
    {
        private readonly IUserService _userService;
        public AuthenticateController(IUserService userService)
        {
            this._userService = userService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterInputDTO registerInputModel)
        {
            var createdUser = await _userService.Register(registerInputModel);

            return Ok(createdUser);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAsync([FromBody] LoginInputDTO loginInputDTO)
        {
            var user = await this._userService.Login(loginInputDTO);

            return Ok(user);
        }


        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> LogoutAsync()
        {
            try
            {
                await this._userService.Logout();
            }
            catch (Exception ex)
            {

                return this.BadRequest(ex.Message);
            }

            return this.Ok();
        }
    }
}
