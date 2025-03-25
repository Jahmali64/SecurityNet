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
    
    public AuthController(IAuthService authService, ILogger<AuthController> logger) {
        _authService = authService;
        _logger = logger;
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
    public async Task<ActionResult<UserTokenDto?>> Login([FromBody] LoginUserDto request) {
        _logger.LogInformation("Attempting to log in user: {userName}", request.UserName);

        try {
            UserTokenDto? userTokens = await _authService.Login(request);
            if (userTokens is not null) return Ok(userTokens);
            
            _logger.LogWarning("UserName: {userName} not found", request.UserName);
            return BadRequest("Invalid username or password");
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to log in user. Message: {message}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<UserTokenDto>> RefreshToken([FromBody] RequestRefreshTokenDto request) {
        _logger.LogInformation("Attempting to refresh token");
        
        if (request.UserId < 1) return BadRequest("Invalid userId");
        if (string.IsNullOrWhiteSpace(request.RefreshToken)) return BadRequest("Invalid refresh token");

        try {
            UserTokenDto? userTokens = await _authService.RefreshTokens(request);
            if (userTokens is not null && !string.IsNullOrWhiteSpace(userTokens.AccessToken) && !string.IsNullOrWhiteSpace(userTokens.RefreshToken)) return Ok(userTokens);
            _logger.LogWarning("Token {refreshToken} not found", request.RefreshToken);
            return Unauthorized("Invalid credentials");
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to refresh token. Message: {message}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }
}
