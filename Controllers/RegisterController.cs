using Microsoft.AspNetCore.Mvc;
using System.Net;


namespace sweetbackend.Controllers
{
    public class Credential
    {
        public string? User { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
    }
}

namespace sweetbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;


        // POST api/
        [HttpPost]
        public async Task<ActionResult> RegisterUser([FromBody] Credential credential)
        {
            if (credential.User == null || credential.Password == null || credential.Email == null)
            {
                return BadRequest("Username, password and email are required.");
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(credential.Password);

            var user = new User
            {
                UserName = credential.User,
                PasswordHash = hashedPassword,
                Email = credential.Email,  
                Role = credential.Role?? "user", // Default role
                Verified = false,
                Approved = false
            };

            _context.Users.Add(user);
            var response = await _context.SaveChangesAsync();

            if (response == 0)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Failed to register user.");
            }

            return Ok("User registered successfully.");

        }

    }
}
