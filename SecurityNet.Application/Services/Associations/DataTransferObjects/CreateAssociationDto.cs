namespace SecurityNet.Application.Services.Associations.DataTransferObjects;

public sealed class CreateAssociationDto {
    public string Name { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public bool Active { get; set; }
}
