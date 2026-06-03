namespace SimplePartyList.Core.DTOs;

public class CreateItemDto
{
    public string Name { get; set; } = string.Empty;
    public int? MaxQuantity { get; set; }
}
