namespace SecurityNet.API.TestModels;

public sealed class User {
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}
