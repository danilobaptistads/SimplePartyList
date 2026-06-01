namespace SimplePartyList.Core.Entities;

public class ChosenList
{
    public Guid ChosenListId { get; set; } = Guid.NewGuid();
    public Guid ListUrl { get; set; } = Guid.NewGuid();
    public DateTime Expire { get; set; }
    public ICollection<Item> Items { get; set; } = [];
    public ICollection<Chosen> Chosens { get; set; } = [];
}
