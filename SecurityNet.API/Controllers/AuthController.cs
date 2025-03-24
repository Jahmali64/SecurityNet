using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SecurityNet.API.TestModels;

namespace SecurityNet.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class AuthController : ControllerBase {
    private static readonly User s_user = new();
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;
    
    public AuthController(ILogger<AuthController> logger, IConfiguration configuration) {
        _logger = logger;
        _configuration = configuration;
    }
    
    [HttpPost("register")]
    public ActionResult<User> Register([FromBody] UserDto request) {
        string hashedPassword = new PasswordHasher<User>().HashPassword(s_user, request.Password);

        try {
            s_user.Username = request.Username;
            s_user.PasswordHash = hashedPassword;
            return Ok(s_user);
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to create user. Message: {message}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("login")]
    public ActionResult<string> Login([FromBody] UserDto request) {
        if (s_user.Username != request.Username) {
            _logger.LogWarning("Username {username} not match", request.Username);
            return Unauthorized();
        }

        if (new PasswordHasher<User>().VerifyHashedPassword(s_user, s_user.PasswordHash, request.Password) == PasswordVerificationResult.Failed) {
            _logger.LogWarning("Password {password} not match", request.Password);
            return Unauthorized();
        }

        string token = CreateToken(s_user);
        return Ok(token);
    }

    private string CreateToken(User user) {
        IEnumerable<Claim> claims = [ new(ClaimTypes.Name, user.Username) ];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("Jwt:Key")!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
        var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration.GetValue<string>("Jwt:Issuer"),
                audience: _configuration.GetValue<string>("Jwt:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:ExpirationInMinutes")),
                signingCredentials: credentials
                );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}
