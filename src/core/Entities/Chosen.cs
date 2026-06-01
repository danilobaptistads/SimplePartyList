using System.ComponentModel.DataAnnotations;

namespace SimplePartyList.Core.Entities;

public class Chosen
{
    public Guid ChosenId { get; set; } = Guid.NewGuid();

    [Required]
    public string GuestName { get; set; } = string.Empty;

    [Required]
    public string ItemName { get; set; } = string.Empty;
    public Guid ChosenListId { get; set; }

}
