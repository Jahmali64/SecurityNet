using SecurityNet.Application.Users;
using SecurityNet.Application.Users.DataTransferObjects;

namespace SecurityNet.Application.Auth;

public interface IAuthService {
    Task<UserDto?> Register(CreateUserDto request);
}

public sealed class AuthService : IAuthService {
    private readonly IUserService _userService;
    
    public AuthService(IUserService userService) {
        _userService = userService;
    }

    public async Task<UserDto?> Register(CreateUserDto request) {
        if (await _userService.UserNameExists(request.UserName)) return null;
        return await _userService.AddUser(request);
    }
}
