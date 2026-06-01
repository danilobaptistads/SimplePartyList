using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SimplePartyList.Core.Entities;

public class Admin : IdentityUser
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public ICollection<Event> Events { get; set; } = [];
    public ICollection<Item> Items { get; set; } = [];
}
