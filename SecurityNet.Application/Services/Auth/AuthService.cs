using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SecurityNet.Application.Services.Users;
using SecurityNet.Application.Services.Users.DataTransferObjects;
using SecurityNet.Application.Services.UserTokens;
using SecurityNet.Application.Services.UserTokens.DataTransferObjects;
using SecurityNet.Shared.Models;

namespace SecurityNet.Application.Services.Auth;

public interface IAuthService {
    Task<UserDto?> Register(CreateUserDto request);
    Task<UserTokenDto?> Login(LoginUserDto request);
    Task<int?> Logout(string refreshToken);
    Task<UserTokenDto?> RefreshTokens(string refreshToken);
}

public sealed class AuthService : IAuthService {
    private readonly IUserService _userService;
    private readonly IUserTokenService _userTokenService;
    private readonly JwtSettings _jwtSettings;
    
    public AuthService(IUserService userService, IUserTokenService userTokenService, IOptions<JwtSettings> jwtSettings) {
        _userService = userService;
        _userTokenService = userTokenService;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<UserDto?> Register(CreateUserDto request) {
        if (await _userService.UserNameExists(request.UserName)) return null;
        
        string hashedPassword = new PasswordHasher<CreateUserDto>().HashPassword(request, request.Password);
        request.Password = hashedPassword;
        return await _userService.AddUser(request);
    }
    
    public async Task<UserTokenDto?> Login(LoginUserDto request) {
        UserDto? user = await _userService.GetUserByUserName(request.UserName);
        
        if (user is null) return null;
        return new PasswordHasher<UserDto>().VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed ? null : await GenerateUserTokens(user);
    }

    public async Task<int?> Logout(string refreshToken) {
        UserDto? user = await _userService.GetUserByRefreshToken(refreshToken);
        return user is null ? null : await _userTokenService.InvalidateRefreshToken(user.UserId);
    }

    public async Task<UserTokenDto?> RefreshTokens(string refreshToken) {
        UserDto? user = await ValidateRefreshToken(refreshToken);
        return user is null ? null : await GenerateUserTokens(user);
    }
    
    private string CreateToken(UserDto user) {
        IEnumerable<Claim> claims = [
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, string.Join(",", user.Roles)),
        ];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
        var tokenDescriptor = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                signingCredentials: credentials
            );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    private async Task<UserDto?> ValidateRefreshToken(string refreshToken) {
        UserDto? user = await _userService.GetUserByRefreshToken(refreshToken);
        
        if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpirationDate <= DateTime.UtcNow) return null;
        return user;
    }
    
    private async Task<UserTokenDto> GenerateUserTokens(UserDto user) => new(CreateToken(user), await _userTokenService.AddUserToken(user.UserId));
}
