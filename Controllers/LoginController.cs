using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace sweetbackend.Controllers
{
    [Route("api/signin")]
    [ApiController]
    public class LoginController(AppDbContext context, ITokenService tokenService) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly ITokenService _tokenService = tokenService;

        [HttpPost]
        public async Task<ActionResult> UserLogin([FromBody] Credential credential)
        {
            Console.WriteLine(credential.Email);
            Console.WriteLine(credential.Password);

            if (string.IsNullOrEmpty(credential.Password) || string.IsNullOrEmpty(credential.Email))
            {
                return BadRequest("Email and Password are required.");
            }

            try
            {

                User? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == credential.Email);

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
                //     Console.WriteLine("User not verified.");
                //     return Unauthorized("User not verified.");
                // }

                if (!user.Approved)
                {
                    Console.WriteLine("User not approved.");
                    return Unauthorized("User not approved.");
                }

                try
                {
                    var token = this._tokenService.GenerateJwtToken(user.Email);

                    return Ok(new { token });
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, e.Message);
                }
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.Message);
            }
        }

    }
}
