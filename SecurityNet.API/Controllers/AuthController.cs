using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SecurityNet.Application.Auth;
using SecurityNet.Application.Users;
using SecurityNet.Application.Users.DataTransferObjects;

namespace SecurityNet.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class AuthController : ControllerBase {
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;
    
    public AuthController(IAuthService authService, IUserService userService, ILogger<AuthController> logger, IConfiguration configuration) {
        _authService = authService;
        _userService = userService;
        _logger = logger;
        _configuration = configuration;
    }
    
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register([FromBody] CreateUserDto request) {
        _logger.LogInformation("Registering user: {userName}", request.UserName);
        string hashedPassword = new PasswordHasher<CreateUserDto>().HashPassword(request, request.Password);
        request.Password = hashedPassword;

        try {
            UserDto? user = await _authService.Register(request);
            if (user is not null) return Ok(user);
            
            _logger.LogWarning("User {userName} could not be registered", request.UserName);
            return BadRequest();
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to create user. Message: {message}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<string?>> Login([FromBody] LoginUserDto request) {
        _logger.LogInformation("Logging user: {userName}", request.UserName);
        UserDto? user = await _userService.GetUserByUserName(request.UserName);
        
        if (user is null) {
            _logger.LogWarning("Username {username} not match", request.UserName);
            return Unauthorized();
        }

        if (new PasswordHasher<UserDto>().VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed) {
            _logger.LogWarning("Password {password} not match", request.Password);
            return Unauthorized();
        }

        string token = CreateToken(user);
        return Ok(token);
    }

    private string CreateToken(UserDto user) {
        IEnumerable<Claim> claims = [ new(ClaimTypes.Name, user.UserName) ];
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
