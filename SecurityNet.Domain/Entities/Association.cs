namespace SecurityNet.Domain.Entities;

public partial class Association
{
    public int AssociationId { get; set; }

    public string? Name { get; set; }

    public string? Website { get; set; }

    public bool Active { get; set; }

    public bool Trash { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
