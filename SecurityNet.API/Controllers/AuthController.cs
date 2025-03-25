using Microsoft.AspNetCore.Mvc;
using SecurityNet.Application.Auth;
using SecurityNet.Application.Users.DataTransferObjects;
using SecurityNet.Application.UserTokens.DataTransferObjects;

namespace SecurityNet.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class AuthController : ControllerBase {
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;
    
    public AuthController(IAuthService authService, ILogger<AuthController> logger, IConfiguration configuration) {
        _authService = authService;
        _logger = logger;
        _configuration = configuration;
    }
    
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register([FromBody] CreateUserDto request) {
        _logger.LogInformation("Attempting to register user: {userName}", request.UserName);

        try {
            UserDto? user = await _authService.Register(request);
            if (user is not null) return Ok(user);
            
            _logger.LogWarning("User {userName} already registered", request.UserName);
            return BadRequest("Invalid username or password");
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to create user. Message: {message}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<string?>> Login([FromBody] LoginUserDto request) {
        _logger.LogInformation("Attempting to log in user: {userName}", request.UserName);

        try {
            UserTokenDto? userTokens = await _authService.Login(request);
            if (userTokens is null) {
                _logger.LogWarning("UserName: {userName} not found", request.UserName);
                return BadRequest("Invalid username or password");
            }

            var cookieOptions = new CookieOptions {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshTokenExpirationInDays"))
            };
            Response.Cookies.Append("refreshToken", userTokens.RefreshToken, cookieOptions);
            return Ok(userTokens.AccessToken);
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to log in user. Message: {message}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<UserTokenDto>> RefreshToken() {
        _logger.LogInformation("Attempting to refresh token");

        if (!Request.Cookies.TryGetValue("refreshToken", out string? refreshToken)) {
            _logger.LogWarning("No refresh token found");
            return Unauthorized("No refresh token found");
        }

        try {
            UserTokenDto? userTokens = await _authService.RefreshTokens(refreshToken);
            if (userTokens is null || string.IsNullOrWhiteSpace(userTokens.AccessToken)) {
                _logger.LogWarning("Token {refreshToken} not found", refreshToken);
                return Unauthorized("Invalid credentials");
            }

            var cookieOptions = new CookieOptions {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshTokenExpirationInDays"))
            };
            
            Response.Cookies.Append("refreshToken", userTokens.RefreshToken, cookieOptions);
            return Ok(userTokens.AccessToken);
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to refresh token. Message: {message}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }
}
