using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace sweetbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController(AppDbContext context, ITokenService tokenService) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly ITokenService _tokenService = tokenService;

        [HttpGet]
        public async Task<ActionResult> UserLogin([FromQuery] Credential credential)
        {   
            Console.WriteLine(credential.UserName);
            Console.WriteLine(credential.Password);
            
            if (string.IsNullOrEmpty(credential.Password) || string.IsNullOrEmpty(credential.UserName))
            {
                return BadRequest("Username and Password are required.");
            }

            User? user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == credential.UserName);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // compare password with hash
            if (!BCrypt.Net.BCrypt.Verify(credential.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid password.");
            }

            // if (!user.Verified)
            // {
            //     return Unauthorized("User not verified.");
            // }

            // if (!user.Approved)
            // {
            //     return Unauthorized("User not approved.");
            // }
            try
            {
                var token = this._tokenService.GenerateJwtToken(user.UserName);

                return Ok(new { token });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.Message);
            }
        }

    }
}
