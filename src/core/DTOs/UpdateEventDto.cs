using System.ComponentModel.DataAnnotations;

namespace SimplePartyList.Core.DTOs;

public class UpdateEventDto
{
    [Required]
    public Guid EventId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }
}
