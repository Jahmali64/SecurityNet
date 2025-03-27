namespace SecurityNet.Application.Services.Users.DataTransferObjects;

public sealed class LoginUserDto {
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
