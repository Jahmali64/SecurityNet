namespace SecurityNet.Domain.Entities;

public partial class UserToken
{
    public int UserTokenId { get; set; }

    public int UserId { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpirationDate { get; set; }

    public virtual User User { get; set; } = null!;
}
