using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace sweetbackend.Controllers
{
    [Route("api/verify")]
    [ApiController]
    public class VerifyController : ControllerBase
    {
        [HttpGet]
        [Authorize]  // This attribute ensures the user is authenticated
        public IActionResult VerifyToken()
        {
            // If the request reaches here, the token is valid
            var email = User.Claims.Select(c => new { c.Type, c.Value }).ToList()[0].Value;
            Console.WriteLine($"Token is verified. Email: {email}");
            return Ok(new { message = "Token is valid", email });
        }
    }
}
