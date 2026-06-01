namespace SimplePartyList.Core.Entities;

public class Event
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string AdminId { get; set; } = string.Empty;
    public Guid ChosenListId { get; set; }
}
