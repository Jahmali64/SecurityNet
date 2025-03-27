using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SecurityNet.Application.Services.Users;
using SecurityNet.Domain.Entities;
using SecurityNet.Infrastructure.DbContexts;
using SecurityNet.Shared.Models;

namespace SecurityNet.Application.Services.UserTokens;

public interface IUserTokenService {
    Task<string> AddUserToken(int userId);
    Task<int> InvalidateRefreshToken(int userId);
}

public sealed class UserTokenService : IUserTokenService {
    private readonly IDbContextFactory<SecurityNetDbContext> _securityNetDbContextFactory;
    private readonly IUserService _userService;
    private readonly JwtSettings _jwtSettings;
    private readonly CancellationToken _cancellationToken;

    public UserTokenService(IDbContextFactory<SecurityNetDbContext> securityNetDbContextFactory, IUserService userService, IOptions<JwtSettings> jwtSettings, CancellationToken cancellationToken) {
        _securityNetDbContextFactory = securityNetDbContextFactory;
        _userService = userService;
        _jwtSettings = jwtSettings.Value;
        _cancellationToken = cancellationToken;
    }

    public async Task<string> AddUserToken(int userId) {
        if (!await _userService.UserIdExists(userId)) {
            throw new ArgumentException("UserId does not exist");
        }
        string refreshToken = CreateRefreshToken();

        await using SecurityNetDbContext securityNetDbContext = await _securityNetDbContextFactory.CreateDbContextAsync(_cancellationToken);
        UserToken? userToken = await securityNetDbContext.UserTokens.FirstOrDefaultAsync(ut => ut.UserId == userId, _cancellationToken);

        if (userToken is null) {
            userToken = new UserToken { UserId = userId };
            await securityNetDbContext.UserTokens.AddAsync(userToken, _cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(userToken.RefreshToken) || userToken.RefreshTokenExpirationDate <= DateTime.UtcNow) {
            userToken.RefreshToken = refreshToken;
            userToken.RefreshTokenExpirationDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays);
        }
        
        await securityNetDbContext.SaveChangesAsync(_cancellationToken);
        return userToken.RefreshToken;
    }
    
    public async Task<int> InvalidateRefreshToken(int userId) {
        if (!await _userService.UserIdExists(userId)) {
            throw new ArgumentException("UserId does not exist");
        }

        await using SecurityNetDbContext securityNetDbContext = await _securityNetDbContextFactory.CreateDbContextAsync(_cancellationToken);

        return await securityNetDbContext.UserTokens.Where(ut => ut.UserId == userId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(ut => ut.RefreshToken, (string?)null)
                .SetProperty(ut => ut.RefreshTokenExpirationDate, DateTime.UtcNow), _cancellationToken);
    }

    private static string CreateRefreshToken() {
        var randomNumber = new byte[32];

        using var random = RandomNumberGenerator.Create();
        random.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
