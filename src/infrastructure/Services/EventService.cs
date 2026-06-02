using Microsoft.EntityFrameworkCore;
using SimplePartyList.Core.Entities;
using SimplePartyList.Core.Interfaces;
using SimplePartyList.Infrastructure.Data;

namespace SimplePartyList.Infrastructure.Services;

public class EventService : IEventService
{
    private readonly SimplePartyListContext _context;

    public EventService(SimplePartyListContext context)
    {
        _context = context;
    }

    public async Task<Event> CreateAsync(string adminId, string name, DateTime date)
    {
        var chosenList = new ChosenList { Expire = date.AddDays(1),Items = [], Chosens = [] };

        var anEvent = new Event {   AdminId = adminId, Name = name, Date = date, ChosenListId = chosenList.ChosenListId };

        _context.ChosenLists.Add(chosenList);
        _context.Events.Add(anEvent);
        await _context.SaveChangesAsync();

        return anEvent;
    }

    public async Task<Event?> GetByIdAsync(Guid eventId)
    {
        return await _context.Events.FindAsync(eventId);
    }

    public async Task<List<Event>> GetByAdminIdAsync(string adminId)
    {
        return await _context.Events
            .Where(e => e.AdminId == adminId)
            .ToListAsync();
    }

    public async Task<Event> UpdateAsync(Event eventToUpdate)
    {
        var anEvent = await _context.Events.FindAsync(eventToUpdate.EventId)
            ?? throw new KeyNotFoundException($"Event {eventToUpdate.EventId} not found.");

        anEvent.Name = eventToUpdate.Name;
        anEvent.Date = eventToUpdate.Date;

        if (anEvent.ChosenListId != Guid.Empty)
        {
            var chosenList = await _context.ChosenLists.FindAsync(anEvent.ChosenListId);
            if (chosenList is not null)
            {
                chosenList.Expire = eventToUpdate.Date.AddDays(1);
            }
        }

        await _context.SaveChangesAsync();

        return anEvent;
    }

    public async Task DeleteAsync(Event eventToDelete)
    {
        var anEvent = await _context.Events.FindAsync(eventToDelete.EventId)
            ?? throw new KeyNotFoundException($"Event {eventToDelete.EventId} not found.");

        if (anEvent.ChosenListId != Guid.Empty)
        {
            var chosenList = await _context.ChosenLists
                .Include(cl => cl.Chosens)
                .FirstOrDefaultAsync(cl => cl.ChosenListId == anEvent.ChosenListId);

            if (chosenList is not null)
            {
                _context.Chosens.RemoveRange(chosenList.Chosens);
                _context.ChosenLists.Remove(chosenList);
            }
        }

        _context.Events.Remove(anEvent);
        await _context.SaveChangesAsync();
    }
}
