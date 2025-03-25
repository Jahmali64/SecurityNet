namespace SecurityNet.Application.UserTokens.DataTransferObjects;

public sealed class UserTokenDto(string accessToken, string refreshToken) {
    public string AccessToken { get; set; } = accessToken;
    public string RefreshToken { get; set; } = refreshToken;
}
