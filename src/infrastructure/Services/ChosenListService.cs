using Microsoft.EntityFrameworkCore;
using SimplePartyList.Core.Entities;
using SimplePartyList.Core.Interfaces;
using SimplePartyList.Infrastructure.Data;

namespace SimplePartyList.Infrastructure.Services;

public class ChosenListService : IChosenListService
{
    private readonly SimplePartyListContext _context;

    public ChosenListService(SimplePartyListContext context)
    {
        _context = context;
    }

    public async Task<ChosenList> CreateAsync(Guid eventId, DateTime eventDate)
    {
        var chosenList = new ChosenList
        {
            Expire = eventDate.AddDays(1),
            Items = [],
            Chosens = []
        };

        var linkedEvent = await _context.Events.FindAsync(eventId)
            ?? throw new InvalidOperationException($"Event with Id {eventId} not found.");

        linkedEvent.ChosenListId = chosenList.ChosenListId;

        _context.ChosenLists.Add(chosenList);
        await _context.SaveChangesAsync();

        return chosenList;
    }

    public async Task<ChosenList?> GetByListUrlAsync(Guid listUrl)
    {
        return await _context.ChosenLists.FirstOrDefaultAsync(cl => cl.ListUrl == listUrl);
    }

    public async Task<List<ChosenList>> GetByAdminIdAsync(string adminId)
    {
        return await _context.Events
            .Where(e => e.AdminId == adminId)
            .Join(_context.ChosenLists,
                  e => e.ChosenListId,
                  cl => cl.ChosenListId,
                  (e, cl) => cl)
            .ToListAsync();
    }

    public async Task<bool> IsExpiredAsync(Guid listUrl)
    {
        var chosenList = await _context.ChosenLists.FirstOrDefaultAsync(cl => cl.ListUrl == listUrl);
        return chosenList is not null && DateTime.UtcNow > chosenList.Expire;
    }
}
