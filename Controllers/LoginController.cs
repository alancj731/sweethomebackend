using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace sweetbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        [HttpGet]
        public async Task<ActionResult> UserLogin([FromQuery] Credential credential)
        {
            if (string.IsNullOrEmpty(credential.Password) || (string.IsNullOrEmpty(credential.Email) && string.IsNullOrEmpty(credential.User)))
            {
                return BadRequest("Username/email and password are required.");
            }

            User? user = null;
            if (!string.IsNullOrEmpty(credential.Email))
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.Email == credential.Email);
            }

            if ( user == null && !string.IsNullOrEmpty(credential.User))
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == credential.User);
            }

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // compare password with hash
            if (!BCrypt.Net.BCrypt.Verify(credential.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid password.");
            }
            
            return Ok("User login successfully.");

        }


    }
}
