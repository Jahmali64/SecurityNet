using Microsoft.AspNetCore.Mvc;
using SecurityNet.Application.Auth;
using SecurityNet.Application.Users.DataTransferObjects;

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
    public async Task<ActionResult<string?>> Login([FromBody] LoginUserDto request) {
        _logger.LogInformation("Attempting to log in user: {userName}", request.UserName);

        try {
            string? token = await _authService.Login(request);
            if (token is not null) return Ok(token);
            
            _logger.LogWarning("UserName: {userName} or Password: {password} not found", request.UserName, request.Password);
            return BadRequest("Invalid username or password");
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to log in user. Message: {message}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }
}
