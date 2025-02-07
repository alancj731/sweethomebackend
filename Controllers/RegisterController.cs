using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace sweetbackend.Controllers
{
    public class Credential
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
    }
}

namespace sweetbackend.Controllers
{
    [Route("api/signup")]
    [ApiController]
    public class RegisterController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;


        // POST api/
        [HttpPost]
        public async Task<ActionResult> RegisterUser([FromBody] Credential credential)
        {
            if (string.IsNullOrEmpty(credential.Email) || string.IsNullOrEmpty(credential.Password))
            {
                return BadRequest("Email and password are required.");
            }

            try
            {

                // check if user already exists
                User? existUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == credential.Email);

                if (existUser != null)
                {
                    return Conflict("User already exists.");
                }

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(credential.Password);
                var userStoragePath = Path.Join("./UserStorage/", credential.Email.Replace("@", "_").Replace(".", "_"));

                if (!Directory.Exists(userStoragePath))
                {
                    try
                    {
                        Directory.CreateDirectory(userStoragePath);
                    }
                    catch (Exception e)
                    {
                        return StatusCode((int)HttpStatusCode.InternalServerError, e.Message);
                    }
                }

                var user = new User
                {
                    PasswordHash = hashedPassword,
                    Email = credential.Email,
                    Role = credential.Role ?? "user", // Default role
                    StorageFolder = userStoragePath,
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

            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.Message);
            }

        }

    }
}
