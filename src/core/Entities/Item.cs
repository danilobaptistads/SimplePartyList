using System.ComponentModel.DataAnnotations;

namespace SimplePartyList.Core.Entities;

public class Item
{
    public Guid ItemId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public int? MaxQuantity { get; set; } = null;
    public Guid ChosenListId { get; set; }
}
