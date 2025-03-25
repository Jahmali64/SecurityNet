namespace SecurityNet.Domain.Entities;

public partial class UserRole
{
    public int UserRoleId { get; set; }

    public int UserId { get; set; }

    public int RoleId { get; set; }

    public bool Active { get; set; }

    public bool Trash { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
