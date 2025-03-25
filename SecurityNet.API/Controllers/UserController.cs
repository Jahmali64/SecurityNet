using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityNet.Application.Users;
using SecurityNet.Application.Users.DataTransferObjects;

namespace SecurityNet.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UserController : ControllerBase {
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger) {
        _userService = userService;
        _logger = logger;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetUsers() {
        _logger.LogInformation("Getting users");

        try {
            List<UserDto> users = await _userService.GetUsers();
            if (users is { Count: > 0 }) return Ok(users);

            _logger.LogInformation("No users found");
            return NoContent();
        } catch (Exception ex) {
            _logger.LogError("Could not fetch users, Message: {message}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }
}
