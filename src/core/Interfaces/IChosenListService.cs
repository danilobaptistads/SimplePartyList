using SimplePartyList.Core.Entities;

namespace SimplePartyList.Core.Interfaces;

public interface IChosenListService
{
    Task<ChosenList?> GetByListUrlAsync(Guid listUrl);
    Task<ChosenList?> GetByListUrlWithItemsAsync(Guid listUrl);
}
