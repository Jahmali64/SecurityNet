namespace SecurityNet.Application.Services.Associations.DataTransferObjects;

public sealed class AssociationDto {
    public int AssociationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public bool Active { get; set; }
}
