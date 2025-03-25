using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SecurityNet.Application.Users;
using SecurityNet.Application.Users.DataTransferObjects;

namespace SecurityNet.Application.Auth;

public interface IAuthService {
    Task<UserDto?> Register(CreateUserDto request);
    Task<string?> Login(LoginUserDto request);
}

public sealed class AuthService : IAuthService {
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;
    
    public AuthService(IUserService userService, IConfiguration configuration) {
        _userService = userService;
        _configuration = configuration;
    }

    public async Task<UserDto?> Register(CreateUserDto request) {
        if (await _userService.UserNameExists(request.UserName)) return null;
        
        string hashedPassword = new PasswordHasher<CreateUserDto>().HashPassword(request, request.Password);
        request.Password = hashedPassword;
        return await _userService.AddUser(request);
    }
    
    public async Task<string?> Login(LoginUserDto request) {
        UserDto? user = await _userService.GetUserByUserName(request.UserName);
        
        if (user is null) return null;
        return new PasswordHasher<UserDto>().VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed ? null : CreateToken(user);
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
            new(ClaimTypes.Name, user.UserName)
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
}
