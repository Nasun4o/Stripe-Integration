using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace StripeIntegration.Controllers
{
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected string UserId => this.User.FindFirstValue(ClaimTypes.NameIdentifier);
        protected string UserEmail => this.User.FindFirstValue(ClaimTypes.Email);

    }
}
