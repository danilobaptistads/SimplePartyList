namespace SimplePartyList.Core.DTOs;

public class PublicListResponseDto
{
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public bool IsExpired { get; set; }
    public List<ItemDto> Items { get; set; } = [];
}
