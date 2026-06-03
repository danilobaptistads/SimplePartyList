namespace SimplePartyList.Core.DTOs;

public class ItemDto
{
    public Guid ItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? MaxQuantity { get; set; }
    public int ChosenCount { get; set; }
}
