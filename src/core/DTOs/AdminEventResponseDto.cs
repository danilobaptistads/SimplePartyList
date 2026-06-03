namespace SimplePartyList.Core.DTOs;

public class AdminEventResponseDto
{
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public Guid ChosenListId { get; set; }
    public Guid ListUrl { get; set; }
}
