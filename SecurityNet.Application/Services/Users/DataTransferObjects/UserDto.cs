namespace SecurityNet.Application.Services.Users.DataTransferObjects;

public sealed class UserDto {
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool Active { get; set; }
    public List<string> Roles { get; set; } = [];
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpirationDate { get; set; }
}
