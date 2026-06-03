using SimplePartyList.Core.Entities;

namespace SimplePartyList.Core.Interfaces;

public interface IEventService
{
    Task<Event> CreateAsync(string adminId, string name, DateTime date);
    Task<Event?> GetByIdAsync(Guid eventId);
    Task<Event?> GetByChosenListIdAsync(Guid chosenListId);
    Task<List<Event>> GetByAdminIdAsync(string adminId);
    Task<Event> UpdateAsync(Event eventToUpdate);
    Task DeleteAsync(Event eventToDelete);
}
