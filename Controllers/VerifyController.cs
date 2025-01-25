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
            Console.WriteLine("Token is verified");
            return Ok(new { message = "Token is valid" });
        }
    }
}
