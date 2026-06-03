namespace SimplePartyList.Core.DTOs;

public class ChosenResponseDto
{
    public Guid ChosenId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
}
