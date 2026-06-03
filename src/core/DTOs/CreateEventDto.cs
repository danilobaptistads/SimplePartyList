using System.ComponentModel.DataAnnotations;

namespace SimplePartyList.Core.DTOs;

public class CreateEventDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }
}
