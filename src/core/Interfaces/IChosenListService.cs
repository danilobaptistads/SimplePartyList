using SimplePartyList.Core.Entities;

namespace SimplePartyList.Core.Interfaces;

public interface IChosenListService
{
    Task<bool> IsExpiredAsync(Guid listUrl);
    Task<ChosenList?> GetByListUrlAsync(Guid listUrl);
    Task<List<ChosenList>> GetByAdminIdAsync(string adminId);
    Task<ChosenList> CreateAsync(Guid eventId, DateTime eventDate);
    
}
