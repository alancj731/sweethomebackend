using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public interface ITokenService
{
    string GenerateJwtToken(string username);
}

public class TokenService(IConfiguration configuration) : ITokenService
{
    private readonly IConfiguration _configuration = configuration;

    public string GenerateJwtToken(string username)
    {

        if (string.IsNullOrEmpty(username))
        {
            throw new ArgumentException("Username is required for generating a token!", nameof(username));
        }

        var secretKey = this._configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("SecretKey is not configured");
        var minutesToExpireString = this._configuration["JwtSettings:ExpirationInMinutes"] ?? throw new InvalidOperationException("ExpirationInMinutes is not configured");
        if (!double.TryParse(minutesToExpireString, out var minutesToExpire))
        {
            throw new InvalidOperationException("ExpirationInMinutes is not a valid number");
        }
        var issuer = this._configuration["JwtSettings:Issuer"];
        var audience = this._configuration["JwtSettings:Audience"];
        
        // Create a list of claims based on the user data
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            // Add any other claims you need, such as roles, permissions, etc.
        };

        // Set the expiration time for the token (e.g., 1 hour)
        
        var expirationTime = DateTime.UtcNow.AddMinutes(minutesToExpire);

        // Create a symmetric security key from the secret key
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        // Create the signing credentials using the symmetric key
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Create the JWT token
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expirationTime,
            signingCredentials: credentials
        );

        // Return the token as a string
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}