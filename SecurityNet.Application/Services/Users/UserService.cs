using Microsoft.EntityFrameworkCore;
using SecurityNet.Application.Services.Users.DataTransferObjects;
using SecurityNet.Domain.Entities;
using SecurityNet.Infrastructure.DbContexts;

namespace SecurityNet.Application.Services.Users;

public interface IUserService {
    Task<List<UserDto>> GetUsers();
    Task<UserDto?> GetUserByUserId(int userId);
    Task<UserDto?> GetUserByUserName(string userName);
    Task<UserDto?> GetUserByRefreshToken(string refreshToken);
    Task<UserDto> AddUser(CreateUserDto request);
    Task<bool> UserNameExists(string userName);
    Task<bool> UserIdExists(int userId);
}

public sealed class UserService : IUserService {
    private readonly IDbContextFactory<SecurityNetDbContext> _securityNetDbContextFactory;
    private readonly CancellationToken _cancellationToken;

    public UserService(IDbContextFactory<SecurityNetDbContext> securityNetDbContextFactory, CancellationToken cancellationToken) {
        _securityNetDbContextFactory = securityNetDbContextFactory;
        _cancellationToken = cancellationToken;
    }

    public async Task<List<UserDto>> GetUsers() {
        await using SecurityNetDbContext securityNetDbContext = await _securityNetDbContextFactory.CreateDbContextAsync(_cancellationToken);

        return await securityNetDbContext.Users.Where(u => !u.Trash)
            .Select(u => new UserDto {
                UserId = u.UserId,
                UserName = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                PasswordHash = u.PasswordHash ?? string.Empty,
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                Active = u.Active,
                Roles = u.UserRoles.Where(ur => ur.UserId == u.UserId).Select(ur => ur.Role.Name ?? "").ToList()
            }).ToListAsync(_cancellationToken);
    }
    
    public async Task<UserDto?> GetUserByUserId(int userId) {
        await using SecurityNetDbContext securityNetDbContext = await _securityNetDbContextFactory.CreateDbContextAsync(_cancellationToken);

        return await securityNetDbContext.Users.Where(u => u.UserId == userId && !u.Trash)
            .Select(u => new UserDto {
                UserId = u.UserId,
                UserName = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                PasswordHash = u.PasswordHash ?? string.Empty,
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                Active = u.Active,
                Roles = u.UserRoles.Where(ur => ur.UserId == u.UserId).Select(ur => ur.Role.Name ?? "").ToList(),
                RefreshToken = u.UserTokens.Where(ut => ut.UserId == u.UserId).Select(ut => ut.RefreshToken).FirstOrDefault(),
                RefreshTokenExpirationDate = u.UserTokens.Where(ut => ut.UserId == u.UserId).Select(ut => ut.RefreshTokenExpirationDate).FirstOrDefault()
            }).FirstOrDefaultAsync(_cancellationToken);
    }
    
    public async Task<UserDto?> GetUserByRefreshToken(string refreshToken) {
        await using SecurityNetDbContext securityNetDbContext = await _securityNetDbContextFactory.CreateDbContextAsync(_cancellationToken);
        
        return await (
            from u in securityNetDbContext.Users
            join ut in securityNetDbContext.UserTokens on u.UserId equals ut.UserId
            where ut.RefreshToken == refreshToken && !u.Trash
            select new UserDto {
                UserId = u.UserId,
                UserName = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                PasswordHash = u.PasswordHash ?? string.Empty,
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                Active = u.Active,
                Roles = u.UserRoles.Where(ur => ur.UserId == u.UserId).Select(ur => ur.Role.Name ?? "").ToList(),
                RefreshToken = ut.RefreshToken,
                RefreshTokenExpirationDate = ut.RefreshTokenExpirationDate
            }).FirstOrDefaultAsync(_cancellationToken);
    }
    
    public async Task<UserDto?> GetUserByUserName(string userName) {
        await using SecurityNetDbContext securityNetDbContext = await _securityNetDbContextFactory.CreateDbContextAsync(_cancellationToken);

        return await securityNetDbContext.Users.Where(u => u.UserName == userName && !u.Trash)
            .Select(u => new UserDto {
                UserId = u.UserId,
                UserName = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                PasswordHash = u.PasswordHash ?? string.Empty,
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                Active = u.Active,
                Roles = u.UserRoles.Where(ur => ur.UserId == u.UserId).Select(ur => ur.Role.Name ?? "").ToList(),
                RefreshToken = u.UserTokens.Where(ut => ut.UserId == u.UserId).Select(ut => ut.RefreshToken).FirstOrDefault(),
                RefreshTokenExpirationDate = u.UserTokens.Where(ut => ut.UserId == u.UserId).Select(ut => ut.RefreshTokenExpirationDate).FirstOrDefault()
            }).FirstOrDefaultAsync(_cancellationToken);
    }

    public async Task<UserDto> AddUser(CreateUserDto request) {
        await using SecurityNetDbContext securityNetDbContext = await _securityNetDbContextFactory.CreateDbContextAsync(_cancellationToken);

        User user = new() {
            UserName = request.UserName,
            Email = request.Email,
            PasswordHash = request.Password,
            PhoneNumber = request.PhoneNumber,
            Active = true,
            Trash = false,
            CreatedAt = DateTime.Now
        };
        
        await securityNetDbContext.Users.AddAsync(user, _cancellationToken);
        await securityNetDbContext.SaveChangesAsync(_cancellationToken);

        return new UserDto {
            UserId = user.UserId,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PasswordHash = user.PasswordHash,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Active = user.Active
        };
    }

    public async Task<bool> UserNameExists(string userName) {
        await using SecurityNetDbContext securityNetDbContext = await _securityNetDbContextFactory.CreateDbContextAsync(_cancellationToken);
        
        return await securityNetDbContext.Users.AnyAsync(u => u.UserName == userName, _cancellationToken);
    }
    
    public async Task<bool> UserIdExists(int userId) {
        await using SecurityNetDbContext securityNetDbContext = await _securityNetDbContextFactory.CreateDbContextAsync(_cancellationToken);
        
        return await securityNetDbContext.Users.AnyAsync(u => u.UserId == userId, _cancellationToken);
    }
}
