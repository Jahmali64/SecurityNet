using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SecurityNet.Application.Users;
using SecurityNet.Application.Users.DataTransferObjects;
using SecurityNet.Application.UserTokens;
using SecurityNet.Application.UserTokens.DataTransferObjects;

namespace SecurityNet.Application.Auth;

public interface IAuthService {
    Task<UserDto?> Register(CreateUserDto request);
    Task<UserTokenDto?> Login(LoginUserDto request);
    Task<int?> Logout(string refreshToken);
    Task<UserTokenDto?> RefreshTokens(string refreshToken);
}

public sealed class AuthService : IAuthService {
    private readonly IUserService _userService;
    private readonly IUserTokenService _userTokenService;
    private readonly IConfiguration _configuration;
    
    public AuthService(IUserService userService, IUserTokenService userTokenService, IConfiguration configuration) {
        _userService = userService;
        _userTokenService = userTokenService;
        _configuration = configuration;
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
        string? securityKey = _configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(securityKey)) throw new InvalidOperationException("SecurityKey is missing");
        
        string? issuer = _configuration["Jwt:Issuer"];
        if (string.IsNullOrWhiteSpace(issuer)) throw new InvalidOperationException("Issuer is missing");
        
        string? audience = _configuration["Jwt:Audience"];
        if (string.IsNullOrWhiteSpace(audience)) throw new InvalidOperationException("Audience is missing");
        
        if (!int.TryParse(_configuration["Jwt:ExpirationInMinutes"], out int expirationInMinutes)) throw new InvalidOperationException("ExpirationInMinutes is invalid");
        
        IEnumerable<Claim> claims = [
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, string.Join(",", user.Roles)),
        ];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
        var tokenDescriptor = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationInMinutes),
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
