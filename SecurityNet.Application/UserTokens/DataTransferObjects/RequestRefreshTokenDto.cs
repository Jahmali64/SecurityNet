namespace SecurityNet.Application.UserTokens.DataTransferObjects;

public sealed class RequestRefreshTokenDto {
    public int UserId { get; set; }
    public required string RefreshToken { get; set; }
}
